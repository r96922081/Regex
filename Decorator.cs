public class Decorator
{
    public static string Decorate(string re)
    {
        string newRe = "";

        re = Unescape(re);

        List<string> tokens = SplitToken(re);

        for (int i = 0; i < tokens.Count; i++)
        {
            string token = tokens[i];

            if (token.StartsWith("("))
            {
                token = "(" + Decorate(token.Substring(1, token.Length - 2)) + ")";
            }

            string nextToken = "";
            if (i < tokens.Count - 1)
                nextToken = tokens[i + 1];

            if (nextToken == "+")
            {
                token = Decorate(token);
                newRe += token + token + "*";
                i += 1;
            }
            else if (nextToken == "?")
            {
                // A? => (|A)
                token = Decorate(token);
                newRe += "(|" + token + ")";
                i += 1;
            }
            else if (nextToken.StartsWith("{"))
            {
                token = Decorate(token);

                nextToken = nextToken.Substring(1, nextToken.Length - 2);

                int dash = nextToken.IndexOf('-');

                if (dash == -1)
                {
                    int count = int.Parse(nextToken);
                    for (int j = 0; j < count; j++)
                    {
                        newRe += token;
                    }
                }
                else
                {
                    int start = int.Parse(nextToken.Substring(0, dash));
                    int end = int.Parse(nextToken.Substring(dash + 1, nextToken.Length - dash - 1));

                    // A{2-4} = ((AA|AAA)|AAAA)
                    string newToken2 = "";
                    for (int j = start; j <= end; j++)
                    {
                        string newToken3 = "";
                        for (int k = 0; k < j; k++)
                        {
                            newToken3 += token; // create AAA
                        }

                        if (j == start)
                        {
                            newToken2 = newToken3;
                        }
                        else
                        {
                            newToken2 = "(" + newToken2 + "|" + newToken3 + ")";
                        }
                    }

                    newRe += newToken2;
                }

                i += 1;
            }
            else
            {
                newRe += token;
            }
        }

        newRe = DecorateOr(newRe);

        return newRe;
    }

    public static string Unescape(string re)
    {
        string newRe = "";

        for (int i = 0; i < re.Length; i++)
        {
            char c = re[i];
            if (c != '\\')
            {
                newRe += c;
            }
            else
            {
                char c2 = re[i + 1];
                if (c2 == 'n')
                {
                    newRe += "\n";
                    i++;
                }
                else if (c2 == 'r')
                {
                    newRe += "\r";
                    i++;
                }
                else if (c2 == 't')
                {
                    newRe += "\t";
                    i++;
                }
                else if (c2 == '\\')
                {
                    newRe += "\\";
                    i++;
                }
                else
                {
                    newRe += "\\";
                }
            }
        }

        return newRe;
    }

    // AB+(C(D)E){1-3}F => A, B, +, (C(D)E), {1-3}, F
    public static List<string> SplitToken(string re)
    {
        List<string> tokens = new List<string>();
        string braceToken = "";
        int braceLevel = 0;

        for (int i = 0; i < re.Length; i++)
        {
            char c = re[i];

            if (c == '(')
            {
                braceToken += c;
                braceLevel += 1;
            }
            else if (c == ')')
            {
                braceToken += c;
                braceLevel -= 1;
                if (braceLevel == 0)
                {
                    tokens.Add(braceToken);
                    braceToken = "";
                }
            }
            else
            {
                if (braceLevel == 0)
                {
                    if (c == '{')
                    {
                        string token = "";
                        while (true)
                        {
                            char c2 = re[i];
                            token += c2;
                            if (c2 == '}')
                                break;
                            i++;
                        }
                        tokens.Add(token);
                    }
                    else
                    {
                        tokens.Add("" + c);
                    }
                }
                else
                {
                    braceToken += c;
                }
            }
        }

        return tokens;
    }

    // a "|" must accompany with a ()
    // A|B|C -> ((A|B)|C)
    public static string DecorateOr(string re)
    {
        string newRe = "";

        int braceLevel = 0;

        List<string> tokens = new List<string>();
        string token = "";

        for (int i = 0; i < re.Length; i++)
        {
            char c = re[i];

            if (c == '(')
            {
                token += c;
                braceLevel += 1;
            }
            else if (c == ')')
            {
                token += c;
                braceLevel -= 1;
            }
            else if (c == '|')
            {
                if (braceLevel == 0)
                {
                    tokens.Add(token);
                    token = "";
                }
                else
                {
                    token += c;
                }
            }
            else
            {
                token += c;
            }
        }

        tokens.Add(token);

        if (tokens.Count == 1)
        {
            newRe = tokens[0];
        }
        else
        {
            for (int i = 0; i < tokens.Count; i++)
            {
                if (i == 0)
                {
                    newRe += tokens[i];
                }
                else
                {
                    newRe = "(" + newRe + "|" + tokens[i] + ")";
                }
            }
        }

        return newRe;
    }

}

