/*
- Use conventional escape rule, currently, it needs to escape for 15 meta chars
\ ^ | . $ ? * + ( ) [ {  ] } -

===

but in real re, it needs to escape only 12 chars \ ^ | . $ ? * + ( ) [ {


===

not match 
[^A-Z0-9]

===

do not plan to support
\b	Match a word boundary
\B	Match a non-(word boundary)
\A	Match only at beginning of string (same as ^)
\Z	Match only at end of string (same as $)
*/
