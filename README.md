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
    [A-Z] list all possible matches, A to Z\
\
**Key notes**\
The main idea is to build a NFA (Nondeterministic finite-state automata) and transit state by input.  After reading all input, if one of its state is accepted state, then this NFA accept (recognize) input.\
\
There are 2 major difference between NFA and DFA (Deterministic finite-state automata)\
(1) NFA can be at many states at any time, while DFA can be at only one\
(2) NFA can transit from one state to another even if there is no input (ε transition), while DFA must read one input to transit\
\
Our NFA needs to support only 4 meta characters: (, ), |, *,  others are short hands of thier combination, for example:\
A+ = (A)(A)\*\
A? = (|A)\
A{2-4} = (AA|AAA|AAAA)\
\
**Build NFA**\
...\
...\
**Transit State**\
Assume input is "AB", then the transit order is \
(1) ε transition (2) 'A' alphabet match transition (3) ε transition (4) 'B' alphabet match transition (5) ε transition\
If states become empty after any transition, then this input is not accpeted \
If there is one state is accepted state after doing above 5 transition, then this input is accpeted \
\
**Code Remark**
1. Read those 2 methods first, they are the core concept: NFA.Build() & NFA.Recognize(string txt).  Skip *Decorator class* code first.
2. Assure that one | must be accomanied by parentheses ()
3. *Decorator class* is used to add ( ) when necessary, for example, A|B|C => ((A|B)|C)
4. *Decorator class* is also in charge of modifying regular expression before feeding it into NFA, for example, A{2-4} => (AA|AAA|AAAA)\
\
**Reference: Robert Sedgewick, Kevin Wayne, *Algorithhms*, 4th Edition**
