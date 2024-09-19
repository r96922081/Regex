public class Regex
{
    public static bool Recognize(string regex, string input)
    {
        NFA nfa = NFA.Build(regex);
        return nfa.Recognize(input);
    }

    public static String Match(string regex, string input)
    {
        NFA nfa = NFA.Build(regex);
        return nfa.Match(input);
    }

    public static NFA Compile(string regex)
    {
        return NFA.Build(regex);
    }
}

