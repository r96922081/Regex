namespace RegexNs
{

    public class State
    {
        public PatternChar pc = null;
        public int index = -1;
        public List<State> epislonTransition = new List<State>();
        public State matchTransition = null;

        public State(PatternChar pc)
        {
            this.pc = pc;
        }
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
        public List<PatternChar> patternChars = null;

        // One "|" must be used with one ()
        // for example, (((AB|BC)|DE)|FG)
        // so in the operatorStack, when encounter a '|', the next pop must be '('

        public static NFA Build(string pattern)
        {
            NFA nfa = new NFA();

            nfa.patternChars = PatternTransformer.Transform(pattern, ref nfa.startsWith, ref nfa.endsWith);

            List<State> states = new List<State>();
            for (int i = 0; i < nfa.patternChars.Count; i++)
            {
                State s = new State(nfa.patternChars[i]);
                s.index = i;
                states.Add(s);
            }
            State acceptState = new State(new PatternChar('\0', PatternCharType.MetaChar));
            states.Add(acceptState);
            nfa.acceptedState = acceptState;

            nfa.startState = states[0];

            Stack<State> operatorStack = new Stack<State>();

            for (int i = 0; i < nfa.patternChars.Count; i++)
            {
                State s = states[i];
                PatternChar pc = s.pc;
                PatternCharType type = pc.type;


                if (type == PatternCharType.Char || type == PatternCharType.MultipleChar || (pc.c == '.' && type == PatternCharType.MetaChar))
                    s.matchTransition = states[i + 1];
                else if (type == PatternCharType.MetaChar && (pc.c == '(' || pc.c == ')' || pc.c == '*'))
                    s.epislonTransition.Add(states[i + 1]);


                if ((pc.c == '(' && type == PatternCharType.MetaChar) || (pc.c == '|' && type == PatternCharType.MetaChar))
                {
                    operatorStack.Push(s);
                }
                else if (pc.c == ')' && type == PatternCharType.MetaChar)
                {
                    State op = operatorStack.Pop();

                    State nextState = states[i + 1];

                    if (op.pc.c == '|' && op.pc.type == PatternCharType.MetaChar)
                    {
                        State op2 = operatorStack.Pop();
                        op2.epislonTransition.Add(states[op.index + 1]);
                        op.epislonTransition.Add(s);

                        if (nextState.pc.c == '*' && nextState.pc.type == PatternCharType.MetaChar)
                        {
                            nextState.epislonTransition.Add(op2);
                            op2.epislonTransition.Add(nextState);
                        }
                    }
                    else if (op.pc.c == '(' && op.pc.type == PatternCharType.MetaChar)
                    {
                        if (nextState.pc.c == '*' && nextState.pc.type == PatternCharType.MetaChar)
                        {
                            nextState.epislonTransition.Add(op);
                            op.epislonTransition.Add(nextState);
                        }
                    }
                }
                else if (s.pc.type != PatternCharType.MetaChar || (s.pc.c != '(' && s.pc.c != ')' && s.pc.c != '|'))
                {
                    State nextState = states[i + 1];
                    if (nextState.pc.c == '*' && nextState.pc.type == PatternCharType.MetaChar)
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
                if (s.pc.type == PatternCharType.Char)
                {
                    if (s.pc.c == c)
                        nextAvailableStates.Add(s.matchTransition);
                }
                else if (s.pc.type == PatternCharType.MultipleChar)
                {
                    bool match = s.pc.multipleChars.Contains(c);
                    if (s.pc.not)
                        match = !match;

                    if (match)
                        nextAvailableStates.Add(s.matchTransition);
                }
                else if (s.pc.c == '.' && s.pc.type == PatternCharType.MetaChar)
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

}