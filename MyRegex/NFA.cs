public class State
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
    MetaCharacter, // \ ^ | . $ ? * + ( ) [ {   ,   no ] } ?
    None
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

public enum RecognizeResult
{
    AliveAndAccept,
    AliveButNotAccept,
    EndAndReject
}

public class RecognizeParam
{
    public List<State> availableStates;
    public RecognizeParam(List<State> availableStates)
    {
        this.availableStates = availableStates;
    }
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
            ReType type = s.re.type;
            if (type == ReType.Char || type == ReType.MultipleChars || (c == '.' && type == ReType.MetaCharacter))
                s.matchTransition = states[i + 1];
            else if (type == ReType.MetaCharacter && (c == '(' || c == ')' || c == '*'))
                s.epislonTransition.Add(states[i + 1]);

            if ((c == '(' && type == ReType.MetaCharacter) || (c == '|' && type == ReType.MetaCharacter))
            {
                operatorStack.Push(s);
            }
            else if (c == ')' && type == ReType.MetaCharacter)
            {
                State op = operatorStack.Pop();

                State nextState = states[i + 1];

                if (op.re.c == '|' && op.re.type == ReType.MetaCharacter)
                {
                    State op2 = operatorStack.Pop();
                    op2.epislonTransition.Add(states[op.index + 1]);
                    op.epislonTransition.Add(s);

                    if (nextState.re.c == '*' && nextState.re.type == ReType.MetaCharacter)
                    {
                        nextState.epislonTransition.Add(op2);
                        op2.epislonTransition.Add(nextState);
                    }
                }
                else if (op.re.c == '(' && op.re.type == ReType.MetaCharacter)
                {
                    if (nextState.re.c == '*' && nextState.re.type == ReType.MetaCharacter)
                    {
                        nextState.epislonTransition.Add(op);
                        op.epislonTransition.Add(nextState);
                    }
                }
            }
            else if (s.re.type != ReType.MetaCharacter || (s.re.c != '(' && s.re.c != ')' && s.re.c != '|'))
            {
                State nextState = states[i + 1];
                if (nextState.re.c == '*' && nextState.re.type == ReType.MetaCharacter)
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
        Tuple<RecognizeParam, RecognizeResult> ret = InitRecognize();
        RecognizeParam param = ret.Item1;
        RecognizeResult result = ret.Item2;

        int lastMatch = -1;
        for (int i = 0; i < txt.Length; i++)
        {
            result = StepRecognize(txt[i], param);
            if (result == RecognizeResult.AliveAndAccept)
                lastMatch = i;
            else if (result == RecognizeResult.EndAndReject)
                break;
        }

        return txt.Substring(0, lastMatch + 1);
    }

    public RecognizeParam CreateRecognizeParam()
    {
        return new RecognizeParam(new List<State>() { startState });
    }

    public RecognizeResult StepRecognize(char c, RecognizeParam param)
    {
        List<State> availableStates = DoEpsilonTransition(param.availableStates);
        List<State> nextAvailableStates = new List<State>();

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
            else if (s.re.c == '.' && s.re.type == ReType.MetaCharacter)
            {
                nextAvailableStates.Add(s.matchTransition);
            }
        }


        param.availableStates.Clear();
        param.availableStates.AddRange(DoEpsilonTransition(nextAvailableStates));

        if (param.availableStates.Count == 0)
            return RecognizeResult.EndAndReject;
        else if (param.availableStates.Contains(acceptedState))
            return RecognizeResult.AliveAndAccept;
        else
            return RecognizeResult.AliveButNotAccept;
    }

    private Tuple<RecognizeParam, RecognizeResult> InitRecognize()
    {
        List<State> availableStates = new List<State>();
        availableStates.Add(startState);

        RecognizeResult result = RecognizeResult.AliveButNotAccept;
        availableStates = DoEpsilonTransition(availableStates);
        if (availableStates.Contains(acceptedState))
            result = RecognizeResult.AliveAndAccept;

        return new Tuple<RecognizeParam, RecognizeResult>(new RecognizeParam(availableStates), result);
    }

    public bool Recognize(string txt)
    {
        Tuple<RecognizeParam, RecognizeResult> ret = InitRecognize();
        RecognizeParam param = ret.Item1;
        RecognizeResult result = ret.Item2;

        foreach (char c in txt)
        {
            result = StepRecognize(c, param);
            if (result == RecognizeResult.EndAndReject)
                break;
        }

        return result == RecognizeResult.AliveAndAccept;
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
}