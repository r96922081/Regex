
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
(1) Add state for each char of regular expression\
(2) Add additional start & accept state\
(3) Add alphabet transition for alphabet (solid line)\
(4) Add ε transition for meta charater *, (, ), | (dashed line)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.1) For (, ), * add ε transition to next state\
&nbsp;&nbsp;&nbsp;&nbsp;(4.2) For * not after ()\
&nbsp;&nbsp;&nbsp;&nbsp; ![](https://r96922081.github.io/regex/nfa2.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.3) For * after ()\
&nbsp;&nbsp;&nbsp;&nbsp; ![](https://r96922081.github.io/regex/nfa3.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.4) For | \
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/regex/nfa4.png)\

&nbsp;&nbsp;&nbsp;&nbsp;(4.5) Complete example of RE = AB*\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/regex/nfa1.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.6) Complete example of RE = (AB)*\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/regex/nfa5.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.7) Complete example of RE = (A|B)C\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/regex/nfa6.png)\
\
**Transit State**\
Do transition ε and alphabet match in alternative sequence\
Example of Re = AB*, input = ABB
![](https://r96922081.github.io/regex/nfa1.png)
| transition | States after transition |
|--|--|
| ε|  A|
| alphabet match|  B|
| ε|  B, *, Accept|
| alphabet match|*|
| ε|  B, Accept|
\
The last states contain Accept, so it's accepted

**Code Remark**
- Read NFA.Build() & NFA.Recognize(string txt) first.  Skip *Decorator class* code in the beginning
- Assure that one | must be accomanied by parentheses ()
- *Decorator class* is used to add ( ) when necessary, for example, A|B|C => ((A|B)|C)
-  *Decorator class* is also in charge of modifying regular expression before feeding it into NFA, for example, A{2-4} => (AA|AAA|AAAA)
- The code is written for clarity, it was not optimized
- Code did not check RE pattern, it may crash for invalid Re pattern

**Reference: Robert Sedgewick, Kevin Wayne, *Algorithhms*, 4th Edition**
