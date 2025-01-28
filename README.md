



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

## **Key Ideas**

**NFA**

 Use NFA (Non-deterministic finite-state automata) to recognize input\
(1) NFA can be at many states at any time\
(2) NFA can be transited by empty input (ε transition)

**NFA Example**
1.  AB\*\
![](https://r96922081.github.io/Regex/nfa1.png)
2.  (AB)\*\
![](https://r96922081.github.io/Regex/nfa5.png)
3.  (A|B)C\
![](https://r96922081.github.io/Regex/nfa6.png)

**Details of Building NFA**

1. Add state for each char
2. Add alphabet transition for each char (solid line)
3. Add ε transition for meta charater *, (, ), |\
3.1 Add ε transition to (, )\
3.2 If \* is not after (), then add ε transition (dashed line): \
![](https://r96922081.github.io/Regex/nfa2.png)\
3.3 If \* is after (), then\
![](https://r96922081.github.io/Regex/nfa3.png)\
3.4 For | add ε\
![](https://r96922081.github.io/Regex/nfa4.png)


**META characters**

It needs to support only 4 meta characters: (, ), |, *,  others are short hands of thier combination:\
A+ = (A)(A)\*
A? = (|A)
A{2-4} = (AA|AAA|AAAA)

**Reference**

Robert Sedgewick, Kevin Wayne, _Algorithhms_, 4th Edition

