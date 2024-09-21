public class Decorator
{
    public static DecoratedRe Decorate(string re)
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

        List<Re> reList = new List<Re>();
        for (int i = 0; i < re.Length; i++)
            reList.Add(new Re(re[i], ReType.Char));

        decorated.reList = DecorateInternal(reList);

        return decorated;
    }

    public static List<Re> DecorateInternal(List<Re> reList)
    {
        List<Re> newReList = new List<Re>();

        reList = Unescape(reList);

        List<List<Re>> reListTokens = SplitToken(reList);

        for (int i = 0; i < reListTokens.Count; i++)
        {
            List<Re> token = reListTokens[i];

            if (token[0].c == '(')
            {
                token = DecorateInternal(token.GetRange(1, token.Count - 2));
                token.Insert(0, new Re('(', ReType.Char));
                token.Add(new Re(')', ReType.Char));
            }

            List<Re> nextToken = new List<Re>();

            if (i == reListTokens.Count - 1)
            {
                newReList.AddRange(token);
                break;
            }

            nextToken = reListTokens[i + 1];

            if (nextToken[0].c == '+')
            {
                // A+ => AA+

                token = DecorateInternal(token);

                newReList.AddRange(token);
                newReList.AddRange(token);
                newReList.Add(new Re('*', ReType.Char));

                i += 1;
            }
            else if (nextToken[0].c == '?')
            {
                // A? => (|A)
                token = DecorateInternal(token);
                newReList.Add(new Re('(', ReType.Char));
                newReList.Add(new Re('|', ReType.Char));
                newReList.AddRange(token);
                newReList.Add(new Re(')', ReType.Char));

                i += 1;
            }
            else if (nextToken[0].c == '{')
            {
                token = DecorateInternal(token);

                nextToken = nextToken.GetRange(1, nextToken.Count - 2);

                int dash = -1;

                for (int j = 0; j < nextToken.Count; j++)
                {
                    if (nextToken[j].c == '-')
                    {
                        dash = j;
                        break;
                    }
                }

                if (dash == -1)
                {
                    int count = int.Parse("" + nextToken[0].c);
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
                            newReList2.Insert(0, new Re('(', ReType.Char));
                            newReList2.Add(new Re('|', ReType.Char));
                            newReList2.AddRange(newReList3);
                            newReList2.Add(new Re(')', ReType.Char));
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

    public static List<Re> Unescape(List<Re> reList)
    {
        List<Re> newReList = new List<Re>();

        for (int i = 0; i < reList.Count; i++)
        {
            char c = reList[i].c;

            if (c != '\\')
            {
                if (c == '.')
                    newReList.Add(new Re('.', ReType.AllChar));
                else
                    newReList.Add(new Re(c, ReType.Char));
            }
            else
            {
                char c2 = reList[i + 1].c;
                if (c2 == 'n')
                {
                    newReList.Add(new Re('\n', ReType.Char));
                    i++;
                }
                else if (c2 == 'r')
                {
                    newReList.Add(new Re('\r', ReType.Char));
                    i++;
                }
                else if (c2 == 't')
                {
                    newReList.Add(new Re('\t', ReType.Char));
                    i++;
                }
                else if (c2 == '\\')
                {
                    newReList.Add(new Re('\\', ReType.Char));
                    i++;
                }
                else if (c2 == '.')
                {
                    newReList.Add(new Re('.', ReType.Char));
                    i++;
                }
                else
                {
                    newReList.Add(new Re('\\', ReType.Char));
                }
            }
        }

        return newReList;
    }

    // AB+(C(D)E){1-3}F => A, B, +, (C(D)E), {1-3}, F
    public static List<List<Re>> SplitToken(List<Re> reList)
    {
        List<List<Re>> tokens = new List<List<Re>>();
        List<Re> braceToken = new List<Re>();
        int braceLevel = 0;

        for (int i = 0; i < reList.Count; i++)
        {
            Re re = reList[i];

            if (re.c == '(')
            {
                braceToken.Add(re);
                braceLevel += 1;
            }
            else if (re.c == ')')
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
                    if (re.c == '{')
                    {
                        List<Re> token = new List<Re>();
                        while (true)
                        {
                            token.Add(reList[i]);

                            if (reList[i].c == '}')
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
            if (reList[i].c == '(')
            {
                token.Add(reList[i]);
                braceLevel += 1;
            }
            else if (reList[i].c == ')')
            {
                token.Add(reList[i]);
                braceLevel -= 1;
            }
            else if (reList[i].c == '|')
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
                    newReList.Insert(0, new Re('(', ReType.Char));
                    newReList.Add(new Re('|', ReType.Char));
                    newReList.AddRange(tokens[i]);
                    newReList.Add(new Re(')', ReType.Char));
                }
            }
        }

        return newReList;
    }

}

