using System.Diagnostics;

public class Ut
{
    static void Check(bool b)
    {
        if (!b)
            Trace.Assert(false);
    }

    // todo
    // Regex class, with
    // List<sring> Match()
    // bool Recognize()

    // \t, \n
    // ^ => begin with
    // $ => end with
    // [A-Za-z]
    public void RunAllUt()
    {
        UtNfa();
        UtDecorate();
        UtNfaWithDecorator();
        UtMatch();

        Console.WriteLine("UT done!");
    }

    private void UtMatch()
    {
        NFA nfa = NFA.Build("A*");
        Check(nfa.Match("AAABC") == "AAA");
        Check(nfa.Match("B") == "");

        nfa = NFA.Build("(AB)+");
        Check(nfa.Match("A") == "");
        Check(nfa.Match("ABB") == "AB");
        Check(nfa.Match("ABABABB") == "ABABAB");
    }


    private void UtNfaWithDecorator()
    {
        NFA nfa = NFA.Build("AB{2-4}|C");
        Check(nfa.Recognize("C") == true);
        Check(nfa.Recognize("ABB") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("ABBBBB") == false);

        nfa = NFA.Build("((A+)B)+");
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("AABAAB") == true);
        Check(nfa.Recognize("ABBC") == false);
    }

    private void UtSplitToken()
    {
        List<string> tokens = Decorator.SplitToken("AB+(C(D)E){1-3}|F");
        Check(tokens.Count == 7);
        Check(tokens[0] == "A");
        Check(tokens[1] == "B");
        Check(tokens[2] == "+");
        Check(tokens[3] == "(C(D)E)");
        Check(tokens[4] == "{1-3}");
        Check(tokens[5] == "|");
        Check(tokens[6] == "F");
    }

    private void UtDecoratorOr()
    {
        Check(Decorator.DecorateOr("A") == "A");
        Check(Decorator.DecorateOr("AB") == "AB");
        Check(Decorator.DecorateOr("(A)") == "(A)");
        Check(Decorator.DecorateOr("A|B") == "(A|B)");
        Check(Decorator.DecorateOr("A|B|C") == "((A|B)|C)");
        Check(Decorator.DecorateOr("((A|BB)|CCC)") == "((A|BB)|CCC)");
        Check(Decorator.DecorateOr("A|(BC)|D") == "((A|(BC))|D)");
        Check(Decorator.DecorateOr("A|(B|C)") == "(A|(B|C))");
    }

    private void UtDecorateInternal()
    {
        Check(Decorator.Decorate("A") == "A");
        Check(Decorator.Decorate("ABC") == "ABC");
        Check(Decorator.Decorate("A|B") == "(A|B)");

        Check(Decorator.Decorate("A+") == "AA*");
        Check(Decorator.Decorate("A?") == "(|A)");
        Check(Decorator.Decorate("A{3}") == "AAA");
        Check(Decorator.Decorate("A{2-4}") == "((AA|AAA)|AAAA)");

        Check(Decorator.Decorate("BA+") == "BAA*");
        Check(Decorator.Decorate("BA?") == "B(|A)");
        Check(Decorator.Decorate("BA{3}") == "BAAA");
        Check(Decorator.Decorate("BA{2-4}") == "B((AA|AAA)|AAAA)");

        Check(Decorator.Decorate("B(CD)+") == "B(CD)(CD)*");
        Check(Decorator.Decorate("B(CD)?") == "B(|(CD))");
        Check(Decorator.Decorate("B(CD){3}") == "B(CD)(CD)(CD)");
        Check(Decorator.Decorate("B(CD){2-4}") == "B(((CD)(CD)|(CD)(CD)(CD))|(CD)(CD)(CD)(CD))");

        Check(Decorator.Decorate("B|A+") == "(B|AA*)");
        Check(Decorator.Decorate("B|A?") == "(B|(|A))");
        Check(Decorator.Decorate("B|A{3}") == "(B|AAA)");
        Check(Decorator.Decorate("B|A{2-4}") == "(B|((AA|AAA)|AAAA))");

        Check(Decorator.Decorate("((A+)B)+))") == "((AA*)B)((AA*)B)*");
    }

    private void UtDecorate()
    {
        UtSplitToken();
        UtDecoratorOr();
        UtDecorateInternal();
    }

    private void UtNfa()
    {
        string re = "AB";
        NFA nfa = NFA.Build(re);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABA") == false);
        Check(nfa.Recognize("ABB") == false);
        Check(nfa.Recognize("A") == false);

        re = "(A|B)";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("B") == true);
        Check(nfa.Recognize("C") == false);
        Check(nfa.Recognize("AB") == false);

        re = "A*";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("AAA") == true);
        Check(nfa.Recognize("B") == false);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("AAB") == false);
        Check(nfa.Recognize("ABB") == false);
        Check(nfa.Recognize("ABA") == false);

        re = "(AB)*";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABAB") == true);
        Check(nfa.Recognize("ABABABAB") == true);
        Check(nfa.Recognize("ABA") == false);
        Check(nfa.Recognize("ABAA") == false);

        re = "(A|B)*";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABAB") == true);
        Check(nfa.Recognize("ABABAB") == true);
        Check(nfa.Recognize("ABAC") == false);
        Check(nfa.Recognize("C") == false);

        re = "((AB|BC)|DE)";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("BC") == true);
        Check(nfa.Recognize("C") == false);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("E") == false);
        Check(nfa.Recognize("DE") == true);
        Check(nfa.Recognize("ABDE") == false);

        re = "((AB|BC)*|DE)";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("BC") == true);
        Check(nfa.Recognize("DE") == true);
        Check(nfa.Recognize("ABBC") == true);
        Check(nfa.Recognize("ABBCAB") == true);
        Check(nfa.Recognize("ABDE") == false);

        re = "(|A)"; // A?
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("B") == false);

        re = "A(A)*"; // A+
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == false);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("AAA") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("B") == false);

        re = ".";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("") == false);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("B") == true);
        Check(nfa.Recognize("AB") == false);

        re = ".A";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("BA") == true);
        Check(nfa.Recognize("B") == false);
        Check(nfa.Recognize("BAA") == false);
    }
}