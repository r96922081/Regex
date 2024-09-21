


**A [Regular Expression Engine] that supports:**

**Recognize, if the engine accept that input:**

    string datePattern = "[1-9][0-9]{3}-[0-3][0-9]-[0-3][0-9]";
    string dateInput1 = "2024-12-25";
    string dateInput2 = "2024-99-99";
    Trace.Assert(Regex.Recognize(datePattern, dateInput1) == true);
    Trace.Assert(Regex.Recognize(datePattern, dateInput2) == false);

**MatchAll, find all matches:**

    string emailPattern = "[a-zA-Z0-9\\.-]+@[a-zA-Z0-9-](\\.[a-zA-Z0-9-])+";
    string emailInput = "Here are my emails: x96922081x@gmail.com, my-account@yahoo.com. Welcome to mail me";
    List<string> allMatch = Regex.MatchAll(emailPattern, emailInput);
    Trace.Assert(allMatch.Count == 2);
    Trace.Assert(allMatch[0] == "x96922081x@gmail.com");
    Trace.Assert(allMatch[1] == "my-account@yahoo.com");

**Meta-characters:**

    () group
    | or
    . any
    + one or more
    ? zero or one
    * zero or more
    {2-4} occurrence, 2 - 4 times
    ^ begin with
    $ end with
    [A-Z] list all possible matches, A to Z


**Reference: Robert Sedgewick, Kevin Wayne, *Algorithhms*, 4th Edition**

**Key notes**
The main idea is to build a NFA (Nondeterministic finite-state automata) and transit state by input.  After reading all input, if one of its state is accepted state, then this NFA recognize this input.\
