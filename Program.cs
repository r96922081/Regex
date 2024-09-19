using System.Diagnostics;

class Program
{
    static void Main(String[] args)
    {
        Trace.Assert(Regex.Recognize("A*", "") == true);
        Trace.Assert(Regex.Recognize("A+", "") == false);
        Trace.Assert(Regex.Recognize("A*", "AAA") == true);
        Trace.Assert(Regex.Recognize("A+B?", "AAB") == true);
        Trace.Assert(Regex.Recognize("A+B?", "AABB") == false);
        Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBB") == true);
        Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBBB") == false);

        Trace.Assert(Regex.Match("A*", "AAAAB") == "AAAA");
        Trace.Assert(Regex.Match("A*", "B") == "");
        Trace.Assert(Regex.Match("AB{2}", "ABBBBB") == "ABB");

        //new Ut().RunAllUt();
    }
}
