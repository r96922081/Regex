

**A [Regular Expression Engine] that supports:**

**Recognize, if the engine accept that input:**\
Trace.Assert(Regex.Recognize("A*", "") == true);\
Trace.Assert(Regex.Recognize("A+", "") == false);\
Trace.Assert(Regex.Recognize("A*", "AAA") == true);\
Trace.Assert(Regex.Recognize("A+B?", "AAB") == true);\
Trace.Assert(Regex.Recognize("A+B?", "AABB") == false);\
Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBB") == true);\
Trace.Assert(Regex.Recognize("AB{2-4}", "ABBBBBBB") == false);

**Match, find the longest match sub-string from input:**\
Trace.Assert(Regex.Match("A*", "AAAAB") == "AAAA");\
Trace.Assert(Regex.Match("A*", "B") == "");\
Trace.Assert(Regex.Match("AB{2}", "ABBBBB") == "ABB");\
Trace.Assert(Regex.Match("AB|(CD)+", "AAACDCDBBB") == "CDCD");\
Trace.Assert(Regex.Match("^AB+", "ABBC") == "ABB");\
Trace.Assert(Regex.Match("AB+$", "CABB") == "ABB");\
Trace.Assert(Regex.Match("^AB+$", "XABB") == "");

**Meta-characters:**\
() group\
| or\
\. any
\+ one or more\
\? zero or one\
\* zero or more\
{2-4} occurrence, 2 - 4 times\
^ begin with\
$ end with

**Performance is not optimized**

**Reference: Algorithhms 4th Edition by Robert Sedgewick, Kevin Wayne**

