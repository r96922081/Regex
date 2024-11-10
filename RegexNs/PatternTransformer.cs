namespace RegexNs
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

        public PatternChar(char c, PatternCharType type)
        {
            this.c = c;
            this.type = type;
        }

        public char c = '\0';
        public List<char> multipleChars = new List<char>();
        public bool not = false;

        public PatternCharType type = PatternCharType.None;

        public PatternChar Clone()
        {
            PatternChar pc = new PatternChar(c, type);
            pc.not = not;
            pc.multipleChars.AddRange(multipleChars);

            return pc;
        }
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
        public static List<PatternChar> ToPatternChar(string pattern)
        {
            List<PatternChar> patternChars = new List<PatternChar>();
            foreach (char c in pattern)
                patternChars.Add(new PatternChar(c));

            return patternChars;
        }

        // \ ^ | . $ ? * + ( ) [ ] { } d D w W s W
        public static List<PatternChar> TransformEscape(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();

            List<char> escapedChars1 = new List<char>() { '\\', '^', '|', '.', '$', '?', '*', '+', '(', ')', '[', ']', '{', '}', };
            List<char> shorthand = new List<char>() { 'd', 'D', 'w', 'W', 's', 'S' };

            for (int i = 0; i < patternChars.Count; i++)
            {
                PatternChar c = patternChars[i];
                if (c.c == '\\' && i < patternChars.Count)
                {
                    if (escapedChars1.Contains(patternChars[i + 1].c))
                    {
                        newPatternChars.Add(new PatternChar(patternChars[i + 1].c));
                        i++;
                    }
                    else if (shorthand.Contains(patternChars[i + 1].c))
                    {
                        newPatternChars.Add(new PatternChar(patternChars[i + 1].c, PatternCharType.MetaChar));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 'n')
                    {
                        newPatternChars.Add(new PatternChar('\n'));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 'r')
                    {
                        newPatternChars.Add(new PatternChar('\r'));
                        i++;
                    }
                    else if (patternChars[i + 1].c == 't')
                    {
                        newPatternChars.Add(new PatternChar('\t'));
                        i++;
                    }
                }
                else
                {
                    List<char> escapedChars2 = new List<char>() { '\\', '^', '|', '.', '$', '?', '*', '+', '(', ')', '[', ']', '{', '}' };

                    if (escapedChars2.Contains(c.c))
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
                    multipleChar.type = PatternCharType.MultipleChar;

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
          \w	[a-zA-Z0-9]
          \W	[^a-zA-Z0-9]
          \s	[ \t\n\r]
          \S	[^ \t\n\r]
        */
        public static List<PatternChar> TransformShorthand(List<PatternChar> patternChars)
        {
            List<PatternChar> newPatternChars = new List<PatternChar>();
            for (int i = 0; i < patternChars.Count; i++)
            {
                PatternChar pc = patternChars[i];
                if (pc.type == PatternCharType.MetaChar && new List<char> { 'd', 'D', 'w', 'W', 's', 'S' }.Contains(pc.c) && i < patternChars.Count)
                {
                    PatternChar patternChar = new PatternChar();
                    patternChar.type = PatternCharType.MultipleChar;

                    if (pc.c == 'd' || pc.c == 'D')
                    {
                        for (char c2 = '0'; c2 <= '9'; c2++)
                            patternChar.multipleChars.Add(c2);

                        if (pc.c == 'D')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                    else if (pc.c == 'w' || pc.c == 'W')
                    {
                        for (char c2 = '0'; c2 <= '9'; c2++)
                            patternChar.multipleChars.Add(c2);
                        for (char c2 = 'a'; c2 <= 'z'; c2++)
                            patternChar.multipleChars.Add(c2);
                        for (char c2 = 'A'; c2 <= 'Z'; c2++)
                            patternChar.multipleChars.Add(c2);

                        if (pc.c == 'W')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                    else if (pc.c == 's' || pc.c == 'S')
                    {
                        patternChar.multipleChars.Add(' ');
                        patternChar.multipleChars.Add('t');
                        patternChar.multipleChars.Add('n');
                        patternChar.multipleChars.Add('r');

                        if (pc.c == 'S')
                            patternChar.not = true;

                        newPatternChars.Add(patternChar);
                        i++;
                    }
                }
                else
                {
                    newPatternChars.Add(pc);
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
            List<PatternChar> nextNewPatternChars = new List<PatternChar>();
            nextNewPatternChars.AddRange(patternChars);

            bool keepGoing = true;

            while (keepGoing)
            {
                newPatternChars.Clear();
                newPatternChars.AddRange(nextNewPatternChars);
                nextNewPatternChars.Clear();

                keepGoing = false;

                for (int i = newPatternChars.Count - 1; i >= 0; i--)
                {
                    PatternChar c = newPatternChars[i];
                    if (c.type == PatternCharType.MetaChar && (c.c == '+' || c.c == '?' || c.c == '}'))
                    {
                        int j = i;

                        int countStart = -1;
                        int countEnd = -1;

                        if (c.c == '+')
                        {
                            keepGoing = true;
                            j--;
                        }
                        else if (c.c == '?')
                        {
                            keepGoing = true;
                            j--;
                        }
                        else if (c.c == '}')
                        {
                            keepGoing = true;

                            // A{3}
                            // A{3-}
                            // A{2-4}
                            int rightCurlyBracket = j;
                            int leftCurlyBracket = j - 1;
                            for (; !(newPatternChars[leftCurlyBracket].c == '{' && newPatternChars[leftCurlyBracket].type == PatternCharType.MetaChar); leftCurlyBracket--)
                                ;

                            int dash = leftCurlyBracket + 1;
                            for (; newPatternChars[dash].c != '-' && dash < rightCurlyBracket; dash++)
                                ;

                            string temp = "";
                            if (dash == rightCurlyBracket)
                            {
                                // A{3}
                                for (int k = leftCurlyBracket + 1; k < rightCurlyBracket; k++)
                                    temp += newPatternChars[k].c;
                                countStart = int.Parse(temp);
                                countEnd = countStart;
                            }
                            else if (dash + 1 != rightCurlyBracket)
                            {
                                // A{2-4}
                                temp = "";
                                for (int k = leftCurlyBracket + 1; k < dash; k++)
                                    temp += newPatternChars[k].c;
                                countStart = int.Parse(temp);

                                temp = "";
                                for (int k = dash + 1; k < rightCurlyBracket; k++)
                                    temp += newPatternChars[k].c;
                                countEnd = int.Parse(temp);
                            }
                            else
                            {
                                // A{3-}
                                temp = "";
                                for (int k = leftCurlyBracket + 1; k < dash; k++)
                                    temp += newPatternChars[k].c;
                                countStart = int.Parse(temp);
                            }

                            j = leftCurlyBracket - 1;
                        }

                        // a+ => subPattern = a
                        // (abc)+ => subPattern = (abc)
                        List<PatternChar> subPattern = new List<PatternChar>();
                        if (newPatternChars[j].c == ')' && newPatternChars[j].type == PatternCharType.MetaChar)
                        {
                            subPattern.Insert(0, newPatternChars[j]);
                            j--;
                            int level = 1;

                            for (; j >= 0; j--)
                            {
                                if (newPatternChars[j].c == ')' && newPatternChars[j].type == PatternCharType.MetaChar)
                                    level++;
                                else if (newPatternChars[j].c == '(' && newPatternChars[j].type == PatternCharType.MetaChar)
                                    level--;

                                subPattern.Insert(0, newPatternChars[j]);

                                if (level == 0)
                                {
                                    j--;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            subPattern.Add(newPatternChars[j]);
                            j--;
                        }

                        if (c.c == '+')
                        {
                            // A+ = AA*
                            // (abc)+ = (abc)(abc)*
                            nextNewPatternChars.Insert(0, new PatternChar('*', PatternCharType.MetaChar));
                            nextNewPatternChars.InsertRange(0, RepeatPatternChar(subPattern, 2));
                        }
                        else if (c.c == '?')
                        {
                            // A? = (|A)
                            // (abc)? = (|(abc))
                            nextNewPatternChars.Insert(0, new PatternChar(')', PatternCharType.MetaChar));
                            nextNewPatternChars.InsertRange(0, RepeatPatternChar(subPattern, 1));
                            nextNewPatternChars.Insert(0, new PatternChar('|', PatternCharType.MetaChar));
                            nextNewPatternChars.Insert(0, new PatternChar('(', PatternCharType.MetaChar));
                        }
                        else if (c.c == '}')
                        {
                            if (countEnd == -1)
                            {
                                // A{2-} = AAA*
                                // (abc){2-} = (abc)(abc)(abc)*
                                nextNewPatternChars.Insert(0, new PatternChar('*', PatternCharType.MetaChar));
                                nextNewPatternChars.InsertRange(0, RepeatPatternChar(subPattern, countStart + 1));
                            }
                            else if (countStart == countEnd)
                            {
                                // A{3} = AAA
                                // (abc){3} = (abc)(abc)(abc)
                                nextNewPatternChars.InsertRange(0, RepeatPatternChar(subPattern, countStart));
                            }
                            else
                            {
                                // A{2-3} = (AA|AAA)
                                // (abc){2-4} = (((abc)(abc)|(abc)(abc)(abc))|(abc)(abc)(abc)(abc))

                                List<PatternChar> temp = new List<PatternChar>();
                                for (int k = countStart; k <= countEnd; k++)
                                {
                                    if (k != countStart)
                                    {
                                        temp.Insert(0, new PatternChar('(', PatternCharType.MetaChar));
                                        temp.Add(new PatternChar('|', PatternCharType.MetaChar));
                                    }

                                    temp.AddRange(RepeatPatternChar(subPattern, k));

                                    if (k != countStart)
                                        temp.Add(new PatternChar(')', PatternCharType.MetaChar));
                                }

                                nextNewPatternChars.InsertRange(0, temp);
                            }
                        }

                        for (; j >= 0; j--)
                        {
                            nextNewPatternChars.Insert(0, newPatternChars[j]);
                        }
                        break;
                    }
                    else
                    {
                        nextNewPatternChars.Insert(0, c);
                    }
                }
            }

            return nextNewPatternChars;
        }

        private static List<PatternChar> RepeatPatternChar(List<PatternChar> patternChars, int repeatCount)
        {
            List<PatternChar> ret = new List<PatternChar>();

            for (int count = 0; count < repeatCount; count++)
            {
                for (int k = 0; k < patternChars.Count; k++)
                {
                    ret.Add(patternChars[k].Clone());
                }
            }

            return ret;
        }

        public static List<PatternChar> TransformStartsWithEndsWith(List<PatternChar> patternChars, ref bool startsWith, ref bool endsWith)
        {
            if (patternChars.Count > 0 && patternChars[0].c == '^' && patternChars[0].type == PatternCharType.MetaChar)
            {
                startsWith = true;
                patternChars.RemoveAt(0);
            }

            if (patternChars.Count > 0 && patternChars[patternChars.Count - 1].c == '$' && patternChars[patternChars.Count - 1].type == PatternCharType.MetaChar)
            {
                endsWith = true;
                patternChars.RemoveAt(patternChars.Count - 1);
            }


            return patternChars;
        }


        public static List<PatternChar> Transform(string pattern, ref bool startsWith, ref bool endsWith)
        {
            List<PatternChar> patternChars = ToPatternChar(pattern);

            patternChars = TransformEscape(patternChars);
            patternChars = TransformStartsWithEndsWith(patternChars, ref startsWith, ref endsWith);
            patternChars = TransformShorthand(patternChars);
            patternChars = TransformSquareBracket(patternChars);
            patternChars = ModifyParentsisBetweenOr(patternChars);
            patternChars = TransformSuffix(patternChars);

            return patternChars;
        }
    }
}
