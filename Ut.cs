using RegexNs;
using System.Diagnostics;

public class UtAll
{
    public static void Check(bool b)
    {
        if (!b)
            Trace.Assert(false);
    }

    public static void UtTransformEscape()
    {
        // \t\^X\\
        List<PatternChar> pc = PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("\\t\\^X\\\\"));
        Check(pc.Count == 4);
        Check(pc[0].c == '\t');
        Check(pc[3].c == '\\');
    }

    public static void UtTransformShorthand()
    {
        List<PatternChar> pc = PatternTransformer.TransformShorthand(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("\\w")));
        Check(pc[0].type == PatternCharType.MultipleChar);
        Check(pc[0].multipleChars.Count == 62); // a-z A-Z 0-9 = 26 + 26 + 10 = 62

        pc = PatternTransformer.TransformShorthand(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("\\\\w")));
        Check(pc[0].c == '\\');
        Check(pc[1].c == 'w');
    }

    public static void UtTransformSquareBracket()
    {
        List<PatternChar> pc = PatternTransformer.TransformSquareBracket(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("[abc]")));
        Check(pc.Count == 1);
        Check(pc[0].multipleChars.Count == 3);

        pc = PatternTransformer.TransformSquareBracket(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("[-\\na-z-][^-[]\\tx")));
        Check(pc.Count == 4);
        Check(pc[0].multipleChars.Contains('b'));
        Check(pc[0].multipleChars.Contains('-'));
        Check(pc[0].multipleChars.Contains('\n'));
        Check(pc[1].not == true);
        Check(pc[1].multipleChars.Contains('['));
        Check(pc[1].multipleChars.Contains('-'));
        Check(pc[2].c == '\t');
        Check(pc[3].c == 'x');

        pc = PatternTransformer.TransformSquareBracket(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("[+\\t\\n\\r\\\\]")));
        Check(pc[0].multipleChars.Contains('\\'));
        Check(pc[0].multipleChars.Contains('+'));
        Check(pc[0].multipleChars.Contains('\t'));
        Check(pc[0].multipleChars.Count == 5);

        pc = PatternTransformer.TransformSquareBracket(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("[^-]")));
        Check(pc[0].multipleChars.Contains('-'));
        Check(pc[0].multipleChars.Count == 1);
    }

    public static void UtPatternChecker()
    {
        string error = null;

        error = PatternChecker.Check("\\z");
        Check(error.Contains("invalid escape"));
        error = PatternChecker.Check("[\\.]");
        Check(error.Contains("invalid escape"));

        error = PatternChecker.Check("[^]");
        Check(error.Contains("empty []"));
        error = PatternChecker.Check("{}");
        Check(error.Contains("empty {}"));

        error = PatternChecker.Check("a[b");
        Check(error.Contains("unclosed"));
        error = PatternChecker.Check("a(b()");
        Check(error.Contains("unclosed"));
        error = PatternChecker.Check("a{b{}}");
        Check(error.Contains("unclosed"));
        error = PatternChecker.Check("ab[{}()[\\]]");
        Check(error == null);

        error = PatternChecker.Check("a{3}");
        Check(error == null);
        error = PatternChecker.Check("a{1-}");
        Check(error != null);
        error = PatternChecker.Check("a{2-a}");
        Check(error != null);
        error = PatternChecker.Check("a{-}");
        Check(error != null);
    }

    public static void UtModifyParentsisBetweenOr()
    {
        List<PatternChar> pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|b")));
        string s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|b)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(((a|b)))")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|b)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(((a)))")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "a");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(ab)|(cd)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(ab|cd)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("((ab)|(cd))")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(ab|cd)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("|a")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(|a)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(|a)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(|a)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|b|c")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|b)|c)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a|b)|c")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|b)|c)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|(b|c)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|(b|c))");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|(b|c|d)|e")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|((b|c)|d))|e)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|b{1,2}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|b{1,2})");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|(b|c)def")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|(b|c)def)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|def(b|c)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|def(b|c))");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a|b)cd|e")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|b)cd|e)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a|b)(c|d)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a|b)(c|d)");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a|b)|(c|d)")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|b)|(c|d))");

        pc = PatternTransformer.ModifyParentsisBetweenOr(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("a|(b|c)*|d?")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((a|(b|c)*)|d?)");
    }

    /*
    // A+ = AA*
    // A? = (|A)
    // A{2} = AA
    // A{2-3} = (AA|AAA)
    // A{2-} = AAA*
    */
    public static void UtTransformSuffix()
    {
        List<PatternChar> pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("abc")));
        string s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "abc");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A+")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "AA*");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("d(abc)+")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "d(abc)(abc)*");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A?")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(|A)");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(abc)?")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(|(abc))");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A{2}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "AA");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A{2-3}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(AA|AAA)");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A{2-}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "AAA*");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("A{10-11}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(AAAAAAAAAA|AAAAAAAAAAA)");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(abc){2-}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(abc)(abc)(abc)*");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("AB+")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "ABB*");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("((ab){2}){3}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "((ab)(ab))((ab)(ab))((ab)(ab))");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a(b(cd){2}|ef+)){2}")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(a(b(cd)(cd)|eff*))(a(b(cd)(cd)|eff*))");

        pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("((a(b(cd){2}|ef+)){2})?")));
        s = string.Concat(pc.Select(pc2 => pc2.c));
        Check(s == "(|((a(b(cd)(cd)|eff*))(a(b(cd)(cd)|eff*))))");
    }

    private static void UtRecognize()
    {
        string pattern = "A";
        NFA nfa = NFA.Build(pattern);
        Check(nfa.Recognize("A") == true);

        pattern = "AB";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABA") == false);
        Check(nfa.Recognize("ABB") == false);
        Check(nfa.Recognize("A") == false);

        pattern = "(A|B)";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("B") == true);
        Check(nfa.Recognize("C") == false);
        Check(nfa.Recognize("AB") == false);

        pattern = "A*";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("AAA") == true);
        Check(nfa.Recognize("B") == false);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("AAB") == false);
        Check(nfa.Recognize("ABB") == false);
        Check(nfa.Recognize("ABA") == false);

        pattern = "(AB)*";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABAB") == true);
        Check(nfa.Recognize("ABABABAB") == true);
        Check(nfa.Recognize("ABA") == false);
        Check(nfa.Recognize("ABAA") == false);

        pattern = "(A|B)*";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("ABAB") == true);
        Check(nfa.Recognize("ABABAB") == true);
        Check(nfa.Recognize("ABAC") == false);
        Check(nfa.Recognize("C") == false);

        pattern = "((AB|BC)|DE)";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("BC") == true);
        Check(nfa.Recognize("C") == false);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("E") == false);
        Check(nfa.Recognize("DE") == true);
        Check(nfa.Recognize("ABDE") == false);

        pattern = "((AB|BC)*|DE)";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("AB") == true);
        Check(nfa.Recognize("BC") == true);
        Check(nfa.Recognize("DE") == true);
        Check(nfa.Recognize("ABBC") == true);
        Check(nfa.Recognize("ABBCAB") == true);
        Check(nfa.Recognize("ABDE") == false);

        pattern = "(|A)"; // A?
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == true);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("B") == false);

        pattern = "A(A)*"; // A+
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == false);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("AAA") == true);
        Check(nfa.Recognize("AB") == false);
        Check(nfa.Recognize("B") == false);

        pattern = ".";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("") == false);
        Check(nfa.Recognize("A") == true);
        Check(nfa.Recognize("B") == true);
        Check(nfa.Recognize("AB") == false);

        pattern = ".A";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize("AA") == true);
        Check(nfa.Recognize("BA") == true);
        Check(nfa.Recognize("B") == false);
        Check(nfa.Recognize("BAA") == false);

        pattern = "\\.";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize(".") == true);
        Check(nfa.Recognize("A") == false);
        Check(nfa.Recognize("") == false);

        pattern = "\\.A";
        nfa = NFA.Build(pattern);
        Check(nfa.Recognize(".A") == true);
        Check(nfa.Recognize(".") == false);
        Check(nfa.Recognize("..") == false);
        Check(nfa.Recognize("AA") == false);

        nfa = NFA.Build("AB{2-4}|C");
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

        nfa = NFA.Build("[^abc]+");
        Check(nfa.Recognize("a") == false);
        Check(nfa.Recognize("ddd") == true);
        Check(nfa.Recognize("dda") == false);
        Check(nfa.Recognize("dddee") == true);

        nfa = NFA.Build("\\w");
        Check(nfa.Recognize("a") == true);
        Check(nfa.Recognize("@") == false);

        nfa = NFA.Build("\\W");
        Check(nfa.Recognize("a") == false);
        Check(nfa.Recognize("@") == true);

        nfa = NFA.Build("A\tB\nC\rD\\\\E");
        Check(nfa.Recognize("A\tB\nC\rD\\E") == true);

        nfa = NFA.Build("^[1-9][0-9]{3}\\-[0-3][0-9]\\-[0-3][0-9]");
        Check(nfa.Recognize("2024-12-25") == true);
        Check(nfa.Recognize("024-99-99") == false);

        nfa = NFA.Build("'([^']|'')*'");
        Check(nfa.Recognize("''") == true);
        Check(nfa.Recognize("'abc'") == true);
        Check(nfa.Recognize("'a''bc'") == true);
        Check(nfa.Recognize("'a''bc'''") == true);
        Check(nfa.Recognize("'a''bc''") == false);
    }

    private static void UtStepRecognize()
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

    private static void UtMatch()
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

        nfa = NFA.Build("A\\$");
        Check(nfa.Match("BA$B") == "A$");

        nfa = NFA.Build("\\^\\$");
        Check(nfa.Match("^$") == "^$");

        nfa = NFA.Build("^ABC$");
        Check(nfa.Match("ABC") == "ABC");
        Check(nfa.Match("DABC") == "");
        Check(nfa.Match("ABCD") == "");

        nfa = NFA.Build("^AB+$");
        Check(nfa.Match("ABBB") == "ABBB");
        Check(nfa.Match("ABBBC") == "");
        Check(nfa.Match("CABBB") == "");
    }

    private static void UtMatchAll()
    {
        string emailPattern = "[a-zA-Z0-9\\.\\-]+@[a-zA-Z0-9\\-]+(\\.[a-zA-Z0-9\\-]+)+";
        string emailInput = "Here are my emails: x96922081x@gmail.com, my-account@yahoo.com. Welcome to mail me";

        List<string> allMatch = Regex.MatchAll(emailPattern, emailInput);
        Check(allMatch.Count == 2);
        Check(allMatch[0] == "x96922081x@gmail.com");
        Check(allMatch[1] == "my-account@yahoo.com");
    }

    public static void Ut()
    {
        //UtPatternChecker();
        UtTransformEscape();
        UtTransformShorthand();
        UtTransformSquareBracket();
        UtModifyParentsisBetweenOr();
        UtTransformSuffix();
        UtRecognize();
        UtStepRecognize();
        UtMatch();
        UtMatchAll();
        Console.WriteLine("Ut Done");
    }
}
