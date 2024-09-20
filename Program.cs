using System.Diagnostics;

class Program
{
    static void Demo()
    {
        Trace.Assert(Regex.Recognize("A*", "") == true);
        Trace.Assert(Regex.Recognize("A+", "") == false);
        Trace.Assert(Regex.Recognize("A*", "AAA") == true);
        Trace.Assert(Regex.Recognize("A+B?", "AAB") == true);
        Trace.Assert(Regex.Recognize("A+B?", "AABB") == false);
        Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBB") == true);
        Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBBBBB") == false);

        Trace.Assert(Regex.Match("A*", "AAAAB") == "AAAA");
        Trace.Assert(Regex.Match("A*", "B") == "");
        Trace.Assert(Regex.Match("AB{2}", "ABBBBB") == "ABB");
        Trace.Assert(Regex.Match("AB|(CD)+", "AAACDCDBBB") == "CDCD");
        Trace.Assert(Regex.Match("^AB+", "ABBC") == "ABB");
        Trace.Assert(Regex.Match("AB+$", "CABB") == "ABB");
        Trace.Assert(Regex.Match("^AB+$", "XABB") == "");
    }

    static void Ut()
    {
        new Ut().RunAllUt();
    }

    static void Main(String[] args)
    {
        Demo();
    }
}
