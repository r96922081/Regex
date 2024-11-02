public class Decorator
{
    public static DecoratedRe Decorate(string re)
    {
        DecoratedRe decorated = ExtractStartAndEndWith(re);

        decorated.reList = HandleEscape(decorated.reList);

        decorated.reList = DecorateInternal(decorated.reList);

        return decorated;
    }

    public static DecoratedRe ExtractStartAndEndWith(string re)
    {
        DecoratedRe decorated = new DecoratedRe();

        if (re.Length > 0 && re[0] == '^')
        {
            decorated.startsWith = true;
            re = re.Substring(1);
        }

        if (re.Length > 0 && re[re.Length - 1] == '$')
        {
            decorated.endsWith = true;
            re = re.Substring(0, re.Length - 1);
        }

        for (int i = 0; i < re.Length; i++)
            decorated.reList.Add(new Re(re[i], ReType.None));

        return decorated;
    }

    public static List<Re> DecorateInternal(List<Re> reList)
    {
        List<Re> newReList = new List<Re>();

        List<List<Re>> reListTokens = SplitToken(reList);

        for (int i = 0; i < reListTokens.Count; i++)
        {
            List<Re> token = reListTokens[i];

            if (token[0].type == ReType.LeftParentBracket)
            {
                token = DecorateInternal(token.GetRange(1, token.Count - 2));
                token.Insert(0, new Re('(', ReType.LeftParentBracket));
                token.Add(new Re(')', ReType.RightParentBracket));
            }
            else if (token[0].type == ReType.LeftBucketBracket)
            {
                token = token.GetRange(1, token.Count - 2);
                List<char> chars = new List<char>();

                for (int j = 0; j < token.Count; j++)
                {
                    Re re = token[j];

                    Re nextRe = null;
                    Re secondNextRe = null;

                    if (j <= token.Count - 3)
                    {
                        nextRe = token[j + 1];
                        secondNextRe = token[j + 2];
                    }

                    if (nextRe != null && nextRe.type == ReType.Minus)
                    {
                        for (char c = re.c; c <= secondNextRe.c; c++)
                        {
                            chars.Add(c);
                        }
                        j += 2;
                    }
                    else
                    {
                        chars.Add(re.c);
                    }
                }

                token = new List<Re> { new Re(chars, ReType.MultipleChars) };
            }

            List<Re> nextToken = new List<Re>();

            if (i == reListTokens.Count - 1)
            {
                newReList.AddRange(token);
                break;
            }

            nextToken = reListTokens[i + 1];

            if (nextToken[0].type == ReType.Plus)
            {
                // A+ => AA*
                newReList.AddRange(token);
                newReList.AddRange(token);
                newReList.Add(new Re('*', ReType.Star));

                i += 1;
            }
            else if (nextToken[0].type == ReType.Question)
            {
                // A? => (|A)
                newReList.Add(new Re('(', ReType.LeftParentBracket));
                newReList.Add(new Re('|', ReType.Or));
                newReList.AddRange(token);
                newReList.Add(new Re(')', ReType.RightParentBracket));

                i += 1;
            }
            else if (nextToken[0].type == ReType.LeftCurlyBracket)
            {
                nextToken = nextToken.GetRange(1, nextToken.Count - 2);

                int dash = -1;

                for (int j = 0; j < nextToken.Count; j++)
                {
                    if (nextToken[j].type == ReType.Minus)
                    {
                        dash = j;
                        break;
                    }
                }

                if (dash == -1)
                {
                    string s = "";
                    for (int j = 0; j < nextToken.Count; j++)
                        s += nextToken[j].c;

                    int count = int.Parse(s);
                    for (int j = 0; j < count; j++)
                    {
                        newReList.AddRange(token);
                    }
                }
                else
                {
                    string startString = "";
                    for (int j = 0; j < dash; j++)
                        startString += nextToken[j].c;
                    int start = int.Parse(startString);

                    string endString = "";
                    for (int j = dash + 1; j < nextToken.Count; j++)
                        endString += nextToken[j].c;
                    int end = int.Parse(endString);

                    // A{2-4} = ((AA|AAA)|AAAA)
                    List<Re> newReList2 = new List<Re>();
                    for (int j = start; j <= end; j++)
                    {
                        List<Re> newReList3 = new List<Re>();
                        for (int k = 0; k < j; k++)
                        {
                            newReList3.AddRange(token); // create AAA
                        }

                        if (j == start)
                        {
                            newReList2 = newReList3;
                        }
                        else
                        {
                            // (A|AA)
                            newReList2.Insert(0, new Re('(', ReType.LeftParentBracket));
                            newReList2.Add(new Re('|', ReType.Or));
                            newReList2.AddRange(newReList3);
                            newReList2.Add(new Re(')', ReType.RightParentBracket));
                        }
                    }

                    newReList.AddRange(newReList2);
                }

                i += 1;
            }
            else
            {
                newReList.AddRange(token);
            }
        }

        newReList = DecorateOr(newReList);

        return newReList;
    }

    public static List<Re> HandleEscape(List<Re> reList)
    {
        List<Re> newReList = new List<Re>();

        for (int i = 0; i < reList.Count; i++)
        {
            char c = reList[i].c;

            if (c == '\\' && i < reList.Count - 1)
            {
                char c2 = reList[i + 1].c;
                if (new List<char> { '.', '|', '+', '-', '*', '?', '(', ')', '[', ']', '{', '}', '\\', 't', 'n', 'r' }.Contains(c2))
                {
                    if (c2 == 't')
                        newReList.Add(new Re('\t', ReType.Char));
                    else if (c2 == 'n')
                        newReList.Add(new Re('\n', ReType.Char));
                    else if (c2 == 'r')
                        newReList.Add(new Re('\r', ReType.Char));
                    else
                        newReList.Add(new Re(c2, ReType.Char));
                    i++;
                }
                else
                {
                    throw new Exception("Invalid esacpe");
                }

                continue;
            }

            if (c == '.')
                newReList.Add(new Re(c, ReType.AllChar));
            else if (c == '|')
                newReList.Add(new Re(c, ReType.Or));
            else if (c == '+')
                newReList.Add(new Re(c, ReType.Plus));
            else if (c == '-')
                newReList.Add(new Re(c, ReType.Minus));
            else if (c == '*')
                newReList.Add(new Re(c, ReType.Star));
            else if (c == '?')
                newReList.Add(new Re(c, ReType.Question));
            else if (c == '(')
                newReList.Add(new Re(c, ReType.LeftParentBracket));
            else if (c == ')')
                newReList.Add(new Re(c, ReType.RightParentBracket));
            else if (c == '[')
                newReList.Add(new Re(c, ReType.LeftBucketBracket));
            else if (c == ']')
                newReList.Add(new Re(c, ReType.RightBucketBracket));
            else if (c == '{')
                newReList.Add(new Re(c, ReType.LeftCurlyBracket));
            else if (c == '}')
                newReList.Add(new Re(c, ReType.RightCurlyBracket));
            else
                newReList.Add(new Re(c, ReType.Char));
        }

        return newReList;
    }

    // AB+(C(D)E){1-3}F[a-zA-Z] => A, B, +, (C(D)E), {1-3}, F, [a-zA-Z]
    public static List<List<Re>> SplitToken(List<Re> reList)
    {
        List<List<Re>> tokens = new List<List<Re>>();
        List<Re> braceToken = new List<Re>();
        int braceLevel = 0;

        for (int i = 0; i < reList.Count; i++)
        {
            Re re = reList[i];

            if (re.type == ReType.LeftParentBracket)
            {
                braceToken.Add(re);
                braceLevel += 1;
            }
            else if (re.type == ReType.RightParentBracket)
            {
                braceToken.Add(re);
                braceLevel -= 1;
                if (braceLevel == 0)
                {
                    tokens.Add(braceToken);
                    braceToken = new List<Re>();
                }
            }
            else
            {
                if (braceLevel == 0)
                {
                    if (re.type == ReType.LeftCurlyBracket)
                    {
                        List<Re> token = new List<Re>();
                        while (true)
                        {
                            token.Add(reList[i]);

                            if (reList[i].type == ReType.RightCurlyBracket)
                                break;
                            i++;
                        }
                        tokens.Add(token);
                    }
                    else if (re.type == ReType.LeftBucketBracket)
                    {
                        List<Re> token = new List<Re>();
                        while (true)
                        {
                            token.Add(reList[i]);

                            if (reList[i].type == ReType.RightBucketBracket)
                                break;
                            i++;
                        }
                        tokens.Add(token);
                    }
                    else
                    {
                        tokens.Add(new List<Re> { re });
                    }
                }
                else
                {
                    braceToken.Add(re);
                }
            }
        }

        return tokens;
    }

    // a "|" must accompany with a ()
    // A|B|C -> ((A|B)|C)
    public static List<Re> DecorateOr(List<Re> reList)
    {
        List<Re> newReList = new List<Re>();

        int braceLevel = 0;

        List<List<Re>> tokens = new List<List<Re>>();
        List<Re> token = new List<Re>();

        for (int i = 0; i < reList.Count; i++)
        {
            if (reList[i].type == ReType.LeftParentBracket)
            {
                token.Add(reList[i]);
                braceLevel += 1;
            }
            else if (reList[i].type == ReType.RightParentBracket)
            {
                token.Add(reList[i]);
                braceLevel -= 1;
            }
            else if (reList[i].type == ReType.Or)
            {
                if (braceLevel == 0)
                {
                    tokens.Add(token);
                    token = new List<Re>();
                }
                else
                {
                    token.Add(reList[i]);
                }
            }
            else
            {
                token.Add(reList[i]);
            }
        }

        tokens.Add(token);

        if (tokens.Count == 1)
        {
            newReList = tokens[0];
        }
        else
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == 0)
                {
                    newReList.AddRange(tokens[i]);
                }
                else
                {
                    newReList.Insert(0, new Re('(', ReType.LeftParentBracket));
                    newReList.Add(new Re('|', ReType.Or));
                    newReList.AddRange(tokens[i]);
                    newReList.Add(new Re(')', ReType.RightParentBracket));
                }
            }
        }

        return newReList;
    }

}

