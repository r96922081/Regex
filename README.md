


# Regex
Regular expression engine.  It can test if an input is accpeted or rejected by a pattern or find matched substrings of a pattern

**Recognize**

    Ex1:
    Pattern = ((A+)B)+
    AB, AABAAB are accepted
    A, ABBC are rejected
    
    Ex2:
    Pattern = -?\\d+(\\.\\d+)?
    123, 123.456, -123.456 are accepted
    +123, 123x are rejected
    
    Ex3:
    Pattern = [1-9][0-9]{3}-[0-1][0-9]-[0-3][0-9];
    2024-12-25 is accpeted
    2024-99-99,  2024-3-02 are rejected

**Find first match**

    Ex1:
    Pattern = (AB)+
    Input = XABABXABABAB
    First match = ABAB

**Find all matches**

    Ex1:
    Pattern = [a-zA-Z0-9\\.\\-]+@[a-zA-Z0-9\\-]+(\\.[a-zA-Z0-9\\-]+)+    
    Input = My emails: my-account@gmail.com, my-account@yahoo.com. Welcome to mail me
    All matches: my-account@gmail.com, my-account@yahoo.com

## How to Use
**Recognize**


        string pattern = "[1-9][0-9]{3}\\-[0-3][0-9]\\-[0-3][0-9]";
        string Input = "2024-12-25";
        Console.WriteLine(Regex.Recognize(pattern, Input)); // true

**Find all matches**


        string pattern = "[a-zA-Z0-9\\.\\-]+@[a-zA-Z0-9\\-]+(\\.[a-zA-Z0-9\\-]+)+";
        string input = "Here are my emails: my-account@gmail.com, my-account@yahoo.com!";
        List<string> matches = Regex.MatchAll(pattern, input);
        Console.WriteLine(matches[0]); // my-account@gmail.com
        Console.WriteLine(matches[1]); // my-account@yahoo.com

## **Algorithm/Code Explanation**

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
\
(1) Add state for each char of regular expression, and additional start & accept state\
(2) Add alphabet transition for each alphabet (solid line)\
(3) Add ε transition for meta charater *, (, ), | (dashed line)\
&nbsp;&nbsp;&nbsp;&nbsp;(3.1) For (, ), * add ε transition to next state\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(3.1.2) For \* not after (), add additional ε transition: \
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ![](https://r96922081.github.io/images/regex/nfa2.png)\
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;(3.1.2) For \* after (), add additional ε transition: \
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; ![](https://r96922081.github.io/images/regex/nfa3.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(3.2) For | , add ε transition: \
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/images/regex/nfa4.png)\
(4) Complete examples\
&nbsp;&nbsp;&nbsp;&nbsp;(4.1) Complete example of RE = AB\*\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/images/regex/nfa1.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.2) Complete example of RE = (AB)\*\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/images/regex/nfa5.png)\
&nbsp;&nbsp;&nbsp;&nbsp;(4.3) Complete example of RE = (A|B)C\
&nbsp;&nbsp;&nbsp;&nbsp;![](https://r96922081.github.io/images/regex/nfa6.png)\
\
**Transit State**\
\
Do transition ε and alphabet match in alternative sequence\
Example of Re = AB\*, input = ABB
![](https://r96922081.github.io/images/regex/nfa1.png)
| transition | States after transition |
|--|--|
| ε|  A|
| A: alphabet match|  B|
| ε|  B, \*, Accept|
| B: alphabet match|\*|
| ε|  B, \*, Accept|
| B: alphabet match|\*|
| ε|  B, \*, Accept|



The last states contain Accept, so it's accepted

Example of Re = (A|B)C, input = AD
![](https://r96922081.github.io/images/regex/nfa6.png)
| transition | States after transition |
|--|--|
| ε|  A, B|
| A: alphabet match|  \||
| ε|  C|
| D: alphabet match| no any states available|

The last states did not contain Accept, so it's not accepted

**Code Remark**
- Read NFA.Build() & NFA.Recognize(string txt) first.  Skip *Decorator class* code in the beginning
- Assure that one | must be accomanied by parentheses ()
- *Decorator class* is used to add ( ) when necessary, for example, A|B|C => ((A|B)|C)
-  *Decorator class* is also in charge of modifying regular expression before feeding it into NFA, for example, A{2-4} => (AA|AAA|AAAA)
- The code is written for clarity, it was not optimized
- Code did not check RE pattern, it may crash for invalid Re pattern

**Reference: Robert Sedgewick, Kevin Wayne, *Algorithhms*, 4th Edition**
