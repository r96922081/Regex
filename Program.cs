using RegexNs;
using System.Collections.Generic;
using System.Diagnostics;

class Program
{
    static void Recognize(string pattern, string text)
    {
        bool accepted = Regex.Recognize(pattern, text);
        Console.WriteLine("pattern = \"" + pattern + "\", text = \"" + text + "\", accepted = " + accepted);
    }

    static void Match(string pattern, string text)
    {
        List<string> allMatch = Regex.MatchAll(pattern, text);
        Console.WriteLine("pattern = \"" + pattern + "\"");
        Console.WriteLine("text = \"" + text + "\"");
        Console.WriteLine("matched = \"" + string.Join("\", \"", allMatch) + "\"");
    }


    static void Demo()
    {
        Recognize("((AB|BC)*|DE)", "ABBCAB");
        Recognize("((AB|BC)*|DE)", "ABDE");
        Console.WriteLine();

        Recognize("-?\\d+(\\.\\d+)?", "-123");
        Recognize("-?\\d+(\\.\\d+)?", "-123.456");
        Recognize("-?\\d+(\\.\\d+)?", "123.7A");
        Console.WriteLine();

        Recognize("[1-9][0-9]{3}\\-[0-3][0-9]\\-[0-3][0-9]", "2024-12-25");
        Recognize("[1-9][0-9]{3}\\-[0-3][0-9]\\-[0-3][0-9]", "2024-99-99");
        Console.WriteLine();

        Match("[a-zA-Z0-9\\.\\-]+@[a-zA-Z0-9\\-]+(\\.[a-zA-Z0-9\\-]+)+", "Here are my emails: my-account@gmail.com, another12345@yahoo.com. Welcome to mail me");

        Console.WriteLine();
        Match("-?\\d+(\\.\\d+)?", "1.23 + 45.6 * 22 = 1004.43");
    }

    static void Ut()
    {
        UtAll.Ut();
    }

    static void Main(String[] args)
    {
        //Ut();
        Demo();
    }
}
