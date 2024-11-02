using System.Diagnostics;

class Program
{
    static void Demo()
    {
        string datePattern = "[1-9][0-9]{3}\\-[0-3][0-9]\\-[0-3][0-9]";
        string dateInput1 = "2024-12-25";
        string dateInput2 = "2024-99-99";
        Trace.Assert(Regex.Recognize(datePattern, dateInput1) == true);
        Trace.Assert(Regex.Recognize(datePattern, dateInput2) == false);

        string emailPattern = "[a-zA-Z0-9\\.\\-]+@[a-zA-Z0-9\\-]+(\\.[a-zA-Z0-9\\-]+)+";
        string emailInput = "Here are my emails: x96922081x@gmail.com, my-account@yahoo.com. Welcome to mail me";
        List<string> allMatch = Regex.MatchAll(emailPattern, emailInput);
        Trace.Assert(allMatch.Count == 2);
        Trace.Assert(allMatch[0] == "x96922081x@gmail.com");
        Trace.Assert(allMatch[1] == "my-account@yahoo.com");
    }

    static void Ut()
    {
        new Ut().RunAllUt();
    }

    static void Main(String[] args)
    {
        Ut();
        Demo();
    }
}
