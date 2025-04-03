# Regex
Regular expression engine.  It can test if an input is accpeted by a pattern and find matched substrings from input by a pattern

## Test if text is accpeted?

![enter image description here](https://r96922081.github.io/Regex/accept1.png)

## Extract string from text

Extract email:\
![enter image description here](https://r96922081.github.io/Regex/match1.png)
\
\
Extract double number:\
![enter image description here](https://r96922081.github.io/Regex/match2.png)

## **Algorithm**
**Main Idea**

Build a NFA according to pattern, and feed input char by char

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

