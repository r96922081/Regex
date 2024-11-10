using System.Diagnostics;

namespace NewRegex
{
    public class Ut2
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
            List<PatternChar> pc = PatternTransformer.TransformShorthand(PatternTransformer.ToPatternChar("\\w"));
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

            pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(abc){2-}")));
            s = string.Concat(pc.Select(pc2 => pc2.c));
            Check(s == "(abc)(abc)(abc)*");

            pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("AB+")));
            s = string.Concat(pc.Select(pc2 => pc2.c));
            Check(s == "ABB*");

            pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("(a(b(cd){2}|ef+)){2}")));
            s = string.Concat(pc.Select(pc2 => pc2.c));
            Check(s == "(a(b(cd)(cd)|eff*))(a(b(cd)(cd)|eff*))");

            pc = PatternTransformer.TransformSuffix(PatternTransformer.TransformEscape(PatternTransformer.ToPatternChar("((a(b(cd){2}|ef+)){2})?")));
            s = string.Concat(pc.Select(pc2 => pc2.c));
            Check(s == "(|((a(b(cd)(cd)|eff*))(a(b(cd)(cd)|eff*))))");
        }

        public static void Ut()
        {
            //UtPatternChecker();
            UtTransformEscape();
            UtTransformShorthand();
            UtTransformSquareBracket();
            UtModifyParentsisBetweenOr();
            UtTransformSuffix();
        }
    }
}
