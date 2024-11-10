using RegexNs;
using System.Diagnostics;

public class Ut
{
    static void Check(bool b)
    {
        if (!b)
            Trace.Assert(false);
    }

    public void RunAllUt()
    {
        NewRegex.Ut2.Ut();
        UtEscape();
        UtStepRecognize();
        UtRecognize();
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

        nfa = NFA.Build("AB");
        Check(nfa.Match("A") == "");
        Check(nfa.Match("AAABB") == "AB");
        Check(nfa.Match("XABAAA") == "AB");

        nfa = NFA.Build("(AB)+");
        Check(nfa.Match("A") == "");
        Check(nfa.Match("ABB") == "AB");
        Check(nfa.Match("ABABABB") == "ABABAB");
        Check(nfa.Match("XABABABB") == "ABABAB");

        nfa = NFA.Build("^AB+");
        Check(nfa.Match("ABBBC") == "ABBB");
        Check(nfa.Match("XAB") == "");

        nfa = NFA.Build("AB+$");
        Check(nfa.Match("CABBB") == "ABBB");
        Check(nfa.Match("ABBBC") == "");

        nfa = NFA.Build("^AB+$");
        Check(nfa.Match("ABBB") == "ABBB");
        Check(nfa.Match("ABBBC") == "");
        Check(nfa.Match("CABBB") == "");
    }


    private void UtNfaWithDecorator()
    {
        NFA nfa = NFA.Build("AB{2-4}|C");
        Check(nfa.Recognize("C") == true);
        Check(nfa.Recognize("ABB") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("ABBBBB") == false);

        nfa = NFA.Build("A{11}");
        Check(nfa.Recognize("AAAAAAAAAAA") == true);

        nfa = NFA.Build("A{10-20}");
        Check(nfa.Recognize("AAAAAAAAAA") == true);
        Check(nfa.Recognize("AAAAAAAAAAAAAAAAAAAAA") == false);

        nfa = NFA.Build("((A+)B)+");
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("AABAAB") == true);
        Check(nfa.Recognize("ABBC") == false);

        nfa = NFA.Build("[AB]");
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("B") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("C") == false);

        nfa = NFA.Build("[ABCDEFG]+");
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AAAB") == true);
        Check(nfa.Recognize("Z") == false);

        nfa = NFA.Build("[a-zA-Z0-9\\-]");
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("1") == true);
        Check(nfa.Recognize("-") == true);
        Check(nfa.Recognize("@") == false);

        nfa = NFA.Build("A\tB\nC\rD\\\\E");
        Check(nfa.Recognize("A\tB\nC\rD\\E") == true);
    }

    private string GetReString(List<Re> re)
    {
        string s = "";
        foreach (Re r in re)
        {
            s += r.c;
        }
        return s;
    }

    private List<Re> getReList(string re)
    {
        List<Re> list = new List<Re>();
        foreach (char c in re)
        {
            list.Add(new Re(c, ReType.Char));
        }
        return list;
    }

    private void UtSplitToken()
    {
        List<List<Re>> tokens = Decorator.SplitToken(Decorator.HandleEscape(getReList("AB+(C(D)E){1-3}|F[AB]")));
        Check(tokens.Count == 8);
        Check(GetReString(tokens[0]) == "A");
        Check(GetReString(tokens[1]) == "B");
        Check(GetReString(tokens[2]) == "+");
        Check(GetReString(tokens[3]) == "(C(D)E)");
        Check(GetReString(tokens[4]) == "{1-3}");
        Check(GetReString(tokens[5]) == "|");
        Check(GetReString(tokens[6]) == "F");
        Check(GetReString(tokens[7]) == "[AB]");
    }

    private void UtDecoratorOr()
    {
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("A")))) == "A");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("AB")))) == "AB");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("(A)")))) == "(A)");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("A|B")))) == "(A|B)");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("A|B|C")))) == "((A|B)|C)");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("((A|BB)|CCC)")))) == "((A|BB)|CCC)");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("A|(BC)|D")))) == "((A|(BC))|D)");
        Check(GetReString(Decorator.DecorateOr(Decorator.HandleEscape(getReList("A|(B|C)")))) == "(A|(B|C))");
    }


    private void UtDecorateInternal()
    {
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(Decorator.HandleEscape(getReList("A"))))) == "A");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("ABC")))) == "ABC");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("A|B")))) == "(A|B)");

        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("A+")))) == "AA*");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("A?")))) == "(|A)");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("A{3}")))) == "AAA");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("A{2-4}")))) == "((AA|AAA)|AAAA)");

        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("BA+")))) == "BAA*");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("BA?")))) == "B(|A)");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("BA{3}")))) == "BAAA");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("BA{2-4}")))) == "B((AA|AAA)|AAAA)");

        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B(CD)+")))) == "B(CD)(CD)*");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B(CD)?")))) == "B(|(CD))");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B(CD){3}")))) == "B(CD)(CD)(CD)");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B(CD){2-4}")))) == "B(((CD)(CD)|(CD)(CD)(CD))|(CD)(CD)(CD)(CD))");

        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B|A+")))) == "(B|AA*)");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B|A?")))) == "(B|(|A))");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B|A{3}")))) == "(B|AAA)");
        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("B|A{2-4}")))) == "(B|((AA|AAA)|AAAA))");

        Check(GetReString(Decorator.DecorateInternal(Decorator.HandleEscape(getReList("((A+)B)+))")))) == "((AA*)B)((AA*)B)*");
    }

    private void UtDecorate()
    {
        UtSplitToken();
        UtDecoratorOr();
        UtDecorateInternal();
    }

    private void UtEscape()
    {
        string re = "\\\\";
        NFA nfa = NFA.Build(re);
        Check(nfa.Recognize("\\") == true);
        Check(nfa.Recognize("\\\\") == false);

        re = "\\+";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("+") == true);
        Check(nfa.Recognize("\\+") == false);

        re = "\\[";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("[") == true);
        Check(nfa.Recognize("\\[") == false);

        re = "[\t\n]";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("\t") == true);
        Check(nfa.Recognize("\n") == true);

        re = "[ \t\n]*";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("\t") == true);

        re = "[ \t\n]*";
        nfa = NFA.Build(re);
        Check(nfa.Recognize("  \t \n   \t\n ") == true);
    }

    private void UtStepRecognize()
    {
        string re = "A*";
        NFA nfa = NFA.Build(re);
        RecognizeParam param = nfa.CreateRecognizeParam();
        Check(nfa.StepRecognize('A', param) == RecognizeResult.AliveAndAccept);
        Check(nfa.StepRecognize('A', param) == RecognizeResult.AliveAndAccept);
        Check(nfa.StepRecognize('A', param) == RecognizeResult.AliveAndAccept);
        Check(nfa.StepRecognize('B', param) == RecognizeResult.EndAndReject);

        re = "AB";
        nfa = NFA.Build(re);
        param = nfa.CreateRecognizeParam();
        Check(nfa.StepRecognize('A', param) == RecognizeResult.AliveButNotAccept);
        Check(nfa.StepRecognize('B', param) == RecognizeResult.AliveAndAccept);
        Check(nfa.StepRecognize('C', param) == RecognizeResult.EndAndReject);

    }

    private void UtRecognize()
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

        re = "\\.";
        nfa = NFA.Build(re);
        Check(nfa.Recognize(".") == true);
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("") == false);

        re = "\\.A";
        nfa = NFA.Build(re);
        Check(nfa.Recognize(".A") == true);
        Check(nfa.Recognize(".") == false);
        Check(nfa.Recognize("..") == false);
        Check(nfa.Recognize("AA") == false);
    }
}