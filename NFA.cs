﻿public class State
{
    public Re re = null;
    public int index = -1;
    public List<State> epislonTransition = new List<State>();
    public State matchTransition = null;

    public State(Re re)
    {
        this.re = re;
    }
}

public enum ReType
{
    Char, // a, Z, (, ...
    MultipleChars, // [a-zA-Z0-9]
    AllChar, // .
}

public class Re
{
    public char c = '\0';
    public List<char> chars = new List<char>();

    public ReType type = ReType.Char;

    public Re(char c, ReType type)
    {
        this.c = c;
        this.type = type;
    }

    public Re(List<char> chars, ReType type)
    {
        this.chars = chars;
        this.type = type;
    }
}

public class DecoratedRe
{
    public List<Re> reList = new List<Re>();
    public bool startsWith = false;
    public bool endsWith = false;
}

public class NFA
{
    public State startState = null;
    public State acceptedState = null;
    public bool startsWith = false;
    public bool endsWith = false;
    public List<Re> reList = null;

    // case 1: AB
    // case 2: A*
    // case 3: (A|B)
    // case 4: (AB)*
    // A, B stand for regular expression, not only a single alphabet

    // Transition
    // When current char is alphabet, add match transition to next state
    // When current char is (, ), *, add epislon transition to next state
    // When current char is |, *, there are different way to add epislon transtion

    // One "|" must be used with one ()
    // for example, (((AB|BC)|DE)|FG)
    // so in the operatorStack, when encounter a '|', the next pop must be '('

    // short hands
    // A+ = (A)(A)*
    // A? = (|A)
    // A{3} = AAA
    // A{2-4} = (AA|AAA|AAAA)
    // . = any alphabet

    public static NFA Build(string re)
    {
        NFA nfa = new NFA();

        DecoratedRe decorated = Decorator.Decorate(re);
        List<Re> reList = decorated.reList;
        nfa.reList = reList;
        nfa.startsWith = decorated.startsWith;
        nfa.endsWith = decorated.endsWith;

        List<State> states = new List<State>();
        for (int i = 0; i < reList.Count; i++)
        {
            State s = new State(reList[i]);
            s.index = i;
            states.Add(s);
        }
        State acceptState = new State(new Re('\0', ReType.Char));
        states.Add(acceptState);
        nfa.acceptedState = acceptState;

        nfa.startState = states[0];

        Stack<State> operatorStack = new Stack<State>();

        for (int i = 0; i < reList.Count; i++)
        {
            State s = states[i];
            char c = s.re.c;
            if (IsAlphabet(c))
                s.matchTransition = states[i + 1];
            else if (c == '(' || c == ')' || c == '*')
                s.epislonTransition.Add(states[i + 1]);

            if (c == '(' || c == '|')
            {
                operatorStack.Push(s);
            }
            else if (c == ')')
            {
                State op = operatorStack.Pop();

                State nextState = states[i + 1];

                if (op.re.c == '|')
                {
                    State op2 = operatorStack.Pop();
                    op2.epislonTransition.Add(states[op.index + 1]);
                    op.epislonTransition.Add(s);

                    if (nextState.re.c == '*')
                    {
                        nextState.epislonTransition.Add(op2);
                        op2.epislonTransition.Add(nextState);
                    }
                }
                else if (op.re.c == '(')
                {
                    if (nextState.re.c == '*')
                    {
                        nextState.epislonTransition.Add(op);
                        op.epislonTransition.Add(nextState);
                    }
                }
            }
            else if (IsAlphabet(c) || c == '*')
            {
                State nextState = states[i + 1];
                if (nextState.re.c == '*')
                {
                    s.epislonTransition.Add(nextState);
                    nextState.epislonTransition.Add(s);
                }
            }
        }

        return nfa;
    }


    public string Match(string txt)
    {
        for (int i = 0; i < txt.Length; i++)
        {
            string matchString = MatchInternal(txt.Substring(i));

            // found match
            if (matchString != "")
            {
                if (startsWith == true && i != 0)
                    continue;
                if (endsWith == true && i + matchString.Length != txt.Length)
                    continue;
                return matchString;
            }
        }

        return "";
    }

    public string MatchInternal(string txt)
    {
        List<State> availableStates = new List<State>();
        availableStates.Add(startState);
        availableStates = DoEpsilonTransition(availableStates);

        int lastMatch = -1;
        for (int i = 0; i < txt.Length; i++)
        {
            char c = txt[i];
            List<State> nextAvailableStates = new List<State>();

            // check alphabet transition
            foreach (State s in availableStates)
            {
                if (s.re.type == ReType.Char)
                {
                    if (s.re.c == c)
                        nextAvailableStates.Add(s.matchTransition);
                }
                else if (s.re.type == ReType.MultipleChars)
                {
                    foreach (char c2 in s.re.chars)
                    {
                        if (c2 == c)
                        {
                            nextAvailableStates.Add(s.matchTransition);
                            break;
                        }
                    }
                }
                else if (s.re.type == ReType.AllChar)
                {
                    nextAvailableStates.Add(s.matchTransition);
                }
            }

            if (nextAvailableStates.Count == 0)
                return txt.Substring(0, lastMatch + 1);

            availableStates = nextAvailableStates;
            availableStates = DoEpsilonTransition(availableStates);

            if (availableStates.Contains(acceptedState))
                lastMatch = i;
        }

        return txt.Substring(0, lastMatch + 1);
    }

    public bool Recognize(string txt)
    {
        List<State> availableStates = new List<State>();
        availableStates.Add(startState);

        availableStates = DoEpsilonTransition(availableStates);
        foreach (char c in txt)
        {
            List<State> nextAvailableStates = new List<State>();

            // check alphabet transition
            foreach (State s in availableStates)
            {
                if (s.re.type == ReType.Char)
                {
                    if (s.re.c == c)
                        nextAvailableStates.Add(s.matchTransition);
                }
                else if (s.re.type == ReType.MultipleChars)
                {
                    foreach (char c2 in s.re.chars)
                    {
                        if (c2 == c)
                        {
                            nextAvailableStates.Add(s.matchTransition);
                            break;
                        }
                    }
                }
                else if (s.re.type == ReType.AllChar)
                {
                    nextAvailableStates.Add(s.matchTransition);
                }
            }

            availableStates = nextAvailableStates;
            availableStates = DoEpsilonTransition(availableStates);
        }

        return availableStates.Contains(acceptedState);
    }

    private void DFS(State s, HashSet<int> visited, List<State> newStates)
    {
        if (visited.Contains(s.index))
            return;

        visited.Add(s.index);
        newStates.Add(s);
        foreach (State s2 in s.epislonTransition)
            DFS(s2, visited, newStates);
    }

    private List<State> DoEpsilonTransition(List<State> states)
    {
        List<State> newStates = new List<State>();
        HashSet<int> visited = new HashSet<int>();

        foreach (State s in states)
            DFS(s, visited, newStates);

        return newStates;
    }

    private static bool IsAlphabet(char c)
    {
        return c != '(' && c != ')' && c != '*' && c != '|';
    }
}