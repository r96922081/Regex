public class Regex
{
    public static bool Recognize(string regex, string input)
    {
        NFA nfa = NFA.Build(regex);
        return nfa.Recognize(input);
    }

    public static string MatchFirst(string regex, string input)
    {
        NFA nfa = NFA.Build(regex);
        return nfa.Match(input);
    }

    public static List<string> MatchAll(string regex, string input)
    {
        List<string> all = new List<string>();

        NFA nfa = NFA.Build(regex);
        string match = nfa.Match(input);
        while (match != "")
        {
            all.Add(match);
            input = input.Substring(input.IndexOf(match) + match.Length);
            match = nfa.Match(input);
        }

        return all;
    }

    public static NFA Compile(string regex)
    {
        return NFA.Build(regex);
    }
}

