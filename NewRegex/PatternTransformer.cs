using System.Diagnostics;

namespace NewRegex
{
    public class PatternChar
    {
        public PatternChar()
        {
        }

        public PatternChar(char c)
        {
            this.c = c;
            type = PatternCharType.Char;
        }

        public PatternChar(char c, bool escaped)
        {
            this.c = c;
            type = PatternCharType.Char;
            this.escaped = escaped;
        }

        public PatternChar(char c, PatternCharType type)
        {
            this.c = c;
            this.type = type;
        }

        public PatternChar(char c, PatternCharType type, bool escaped)
        {
            this.c = c;
            this.type = type;
            this.escaped = escaped;
        }

        public char c = '\0';
        public List<char> multipleChars = new List<char>();
        public bool not = false;
        public bool escaped = false;

        public PatternCharType type = PatternCharType.None;
    }

    public enum PatternCharType
    {
        Char,
        MultipleChar,
        MetaChar,
        None
    }

    public class PatternTransformer
    {
        static void Check(bool b)
        {
            if (!b)
                Trace.Assert(false);
        }

        public static List<PatternChar> ToPatternChar(string pattern)
        {
            List<PatternChar> patternChars = new List<PatternChar>();
            foreach (char c in pattern)
                patternChars.Add(new PatternChar(c));

            return patternChars;
        }

        // \ ^ | . $ ? * + ( ) [ ] { }
        public static List<PatternChar> TransformEscape(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();
            List<char> escapedChars = new List<char>() { '\\', '^', '|', '.', '$', '?', '*', '+', '(', ')', '[', ']', '{', '}' };

            for (int i = 0; i < patternChars.Count; i++)
            {
                PatternChar c = patternChars[i];
                if (c.c == '\\' && i < patternChars.Count)
                {
                    if (escapedChars.Contains(patternChars[i + 1].c))
                    {
                        newPatternChars.Add(new PatternChar(patternChars[i + 1].c, true));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 'n')
                    {
                        newPatternChars.Add(new PatternChar('\n', true));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 'r')
                    {
                        newPatternChars.Add(new PatternChar('\r', true));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 't')
                    {
                        newPatternChars.Add(new PatternChar('\t', true));
                        i++;
                    }
                }
                else
                {
                    if (escapedChars.Contains(c.c))
                        newPatternChars.Add(new PatternChar(c.c, PatternCharType.MetaChar));
                    else
                        newPatternChars.Add(c);
                }
            }

            return newPatternChars;
        }

        // "[-\\na-z-][^-\\[]\\tx"
        // in [], escape only ], \, \t, \n, \r
        public static List<PatternChar> TransformSquareBracket(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();

            for (int i = 0; i < patternChars.Count;)
            {
                PatternChar c = patternChars[i];

                if (c.c == '[' && c.type == PatternCharType.MetaChar)
                {
                    int squareStart = i;
                    int squareEnd = squareStart + 1;
                    for (; squareEnd < patternChars.Count; squareEnd++)
                    {
                        if (patternChars[squareEnd].c == ']' && patternChars[squareEnd].type == PatternCharType.MetaChar)
                            break;
                    }
                    i = squareEnd + 1;

                    PatternChar multipleChar = new PatternChar();
                    if (patternChars[squareStart + 1].c == '^')
                    {
                        multipleChar.not = true;
                        squareStart++;
                    }

                    for (int j = squareStart + 1; j < squareEnd;)
                    {
                        // [-a] case
                        if (j == squareStart + 1)
                        {
                            if (patternChars[j].c == '-')
                            {
                                multipleChar.multipleChars.Add('-');
                                j++;
                                continue;
                            }
                        }

                        // [-], [a] case
                        if (j + 1 == squareEnd)
                        {
                            multipleChar.multipleChars.Add(patternChars[j].c);
                            break;
                        }

                        // [ab] case
                        if (j + 2 == squareEnd)
                        {
                            multipleChar.multipleChars.Add(patternChars[j].c);
                            multipleChar.multipleChars.Add(patternChars[j + 1].c);
                            break;
                        }

                        if (patternChars[j + 1].c == '-')
                        {
                            for (char c2 = patternChars[j].c; c2 <= patternChars[j + 2].c; c2++)
                                multipleChar.multipleChars.Add(c2);
                            j += 3;
                            continue;
                        }
                        else
                        {
                            multipleChar.multipleChars.Add(patternChars[j].c);
                            j++;
                            continue;
                        }
                    }


                    newPatternChars.Add(multipleChar);
                    continue;
                }
                else
                {
                    newPatternChars.Add(patternChars[i]);
                    i++;
                }
            }

            return newPatternChars;
        }

        /*
          \d	[0-9]
          \D	[^0-9]
          \w	[A-Z0-9]
          \W	[^A-Z0-9]
          \s	[ \t\n\r]
          \S	[^ \t\n\r]
        */
        public static List<PatternChar> TransformShorthand(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();
            for (int i = 0; i < patternChars.Count; i++)
            {
                PatternChar c = patternChars[i];
                if (c.c == '\\' && i < patternChars.Count)
                {
                    if (patternChars[i + 1].c == 'd' || patternChars[i + 1].c == 'D')
                    {
                        PatternChar patternChar = new PatternChar();
                        for (char c2 = '0'; c2 <= '9'; c2++)
                            patternChar.multipleChars.Add(c2);

                        if (patternChars[i + 1].c == 'D')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                    else if (patternChars[i + 1].c == 'w' || patternChars[i + 1].c == 'W')
                    {
                        PatternChar patternChar = new PatternChar();
                        for (char c2 = '0'; c2 <= '9'; c2++)
                            patternChar.multipleChars.Add(c2);
                        for (char c2 = 'a'; c2 <= 'z'; c2++)
                            patternChar.multipleChars.Add(c2);
                        for (char c2 = 'A'; c2 <= 'Z'; c2++)
                            patternChar.multipleChars.Add(c2);

                        if (patternChars[i + 1].c == 'W')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                    else if (patternChars[i + 1].c == 's' || patternChars[i + 1].c == 'S')
                    {
                        PatternChar patternChar = new PatternChar();
                        patternChar.multipleChars.Add(' ');
                        patternChar.multipleChars.Add('t');
                        patternChar.multipleChars.Add('n');
                        patternChar.multipleChars.Add('r');

                        if (patternChars[i + 1].c == 'S')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                }
                else
                {
                    newPatternChars.Add(c);
                }
            }

            return newPatternChars;
        }

        // Add () to each or, ex: a|b|c -> ((a|b)|c)
        // remove redundant () between |, ex: ((ab)|(cd)) -> (ab|cd)
        public static List<PatternChar> ModifyParentsisBetweenOr(List<PatternChar> patternChars)
        {
            List<List<PatternChar>> tokensSplitedByOr = new List<List<PatternChar>>();

            List<PatternChar> token = new List<PatternChar>();
            int level = 0;

            for (int i = 0; i < patternChars.Count; i++)
            {
                PatternChar c = patternChars[i];

                if (c.c == '(' && c.type == PatternCharType.MetaChar)
                    level++;
                else if (c.c == ')' && c.type == PatternCharType.MetaChar)
                    level--;

                if (level > 0)
                {
                    token.Add(c);
                }
                else
                {
                    if (c.c == '|' && c.type == PatternCharType.MetaChar)
                    {
                        tokensSplitedByOr.Add(token);
                        token = new List<PatternChar>();
                    }
                    else
                    {
                        token.Add(c);
                    }
                }
            }

            tokensSplitedByOr.Add(token);

            if (tokensSplitedByOr.Count == 1)
            {
                token = tokensSplitedByOr[0];

                if (token.Count == 0)
                {
                    // the case (|a)
                    return token;
                }

                // if (a)(b), return (a)(b)
                // if abc, return abc
                // if ((a)(b)), return (a)(b)
                if (token[0].c == '(' && token[0].type == PatternCharType.MetaChar)
                {
                    int level2 = 1;
                    for (int j = 1; j < token.Count; j++)
                    {
                        if (token[j].c == '(' && token[j].type == PatternCharType.MetaChar)
                            level2++;
                        else if (token[j].c == ')' && token[j].type == PatternCharType.MetaChar)
                            level2--;


                        if (level2 == 0)
                        {
                            if (j == token.Count - 1)
                            {
                                // the case ((a)(b))
                                token.RemoveAt(0);
                                token.RemoveAt(token.Count - 1);
                                return ModifyParentsisBetweenOr(token);
                            }
                            else
                            {
                                // the case (a)(b)
                                return tokensSplitedByOr[0];
                            }
                        }
                    }

                    return ModifyParentsisBetweenOr(token);
                }
                else
                {
                    return tokensSplitedByOr[0];
                }
            }
            else
            {
                List<PatternChar> ret = ModifyParentsisBetweenOr(tokensSplitedByOr[0]);
                for (int i = 1; i < tokensSplitedByOr.Count; i++)
                {
                    ret.Insert(0, new PatternChar('(', PatternCharType.MetaChar));
                    ret.Add(new PatternChar('|', PatternCharType.MetaChar));
                    ret.AddRange(ModifyParentsisBetweenOr(tokensSplitedByOr[i]));
                    ret.Add(new PatternChar(')', PatternCharType.MetaChar));
                }

                return ret;
            }
        }

        /*
        // A+ = AA*
        // A? = (|A)
        // A{3} = AAA
        // A{2-4} = (AA|AAA|AAAA)
        // A{3-} = AAAA*
        */
        public static List<PatternChar> TransformSuffix(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();

            return newPatternChars;
        }

        public static List<PatternChar> Transform(string pattern)
        {
            List<PatternChar> patternChars = ToPatternChar(pattern);

            patternChars = TransformEscape(patternChars);
            patternChars = TransformShorthand(patternChars);
            patternChars = TransformSquareBracket(patternChars);
            patternChars = ModifyParentsisBetweenOr(patternChars);
            patternChars = TransformSuffix(patternChars);


            return patternChars;
        }
    }
}
