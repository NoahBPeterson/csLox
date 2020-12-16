# csLox
An implementation of Lox interpreted using C#, [following this tutorial.](http://www.craftinginterpreters.com)

#### Control Structures
 - if-then-else
 - while, do-while, for
 - break, continue

#### Data Types
Literal values available are floating point numbers, strings, booleans, and nil.
Classes can be defined with methods and variables encapsulated.

#### Standard Library
`clock()` Returns the system time in seconds.

#### Truthyness
Everything that is not 'nil' or a 'false' value is true.

#### Language Extensions
 - Errors and warnings include the line and character where the issue starts.
 - Multiline comments
 - Comma operator
 - do-while loops
 - continue, break statements
 - The `+` operator can concatenate strings and numbers
 - Lambda functions
 - Static class methods
 - Getter methods
 - Multiple Inheritance
 - Warnings issued for unused or uninitialized variables, and unreachable code
 - Increment and decrement operators `++, --`
 - Nullcheck operators `??, ?.`
 - Cascading operator `..`
 

#### Operator Precedence
Highest to Lowest
```
Name            Operator        Associativity
Call            a()             Left-to-right
postfix         a++, a--        Left-to-right
prefix          ++a, --a        Right-to-left
unary           !a, -a          Right-to-left
factor          /, *            Left-to-right
term            -, +            Left-to-right
comparison      >, >=, <, <=    Left-to-right
equality        !=, ==          Left-to-right
and             and             Left-to-right
or              or              Left-to-right
ternary         a?b:c           Right-to-left
assignment      =               Right-to-left
comma           ,               Left-to-right
```
#### Grammar


###### Statements:
```
program →  declaration* EOF | expression
declaration →  "class" classDeclaration 
            | "fun" function
            |  "var" variableDeclaration
            | statement
classDeclaration →  identifier  ("<" (identifier) ("," identifier)*)? "{" (("class")? function|getter)* "}"
function →  "fun" identifier "(" identifier (, identifier)* ")" block
getter → identifier block
block → "{" declaration* "}"
variableDeclaration → "var" identifier ("=" expression)? (("," term)?)*
statement → print
            | block
            | if | return
            | doWhile | while | for
            | break | continue
            | expressionStatement
print → "print" expression ";"
if → "if" "(" expression ")" statement ("else" statement)?
return →  "return" expression ";"
doWhile → "do" statement "while" "(" expression ")" ";"
while → "while" "(" expression ")" statement
for → "for" "(" (varDeclaration|expressionStatement)? ";" (expression)? ";" (expression)? ")" statement
break → "break" ";"
continue → "continue" ";"
expressionStatement → expression ";" | expression
```
###### Expressions:
```
expression → assignment
assignment → ternaryExpression ("=" assignment)
ternaryExpression → or ("?" equality ":" equality)?
or → and ("or" and)*
and → equality ("and" ternaryExpression)*
equality → comparison (("!="|"==") comparison)*
comparison → term (">"|">="|"<"|"<=" term)*
term → factor ("-"|"+" factor)*
factor → unary ("/"|"*" unary)*
unary → prefix | ("!"|"-" unary)
prefix → ("--"|"++" primary) 
		| postfix
postfix → call 
		| ("--"|"++" call)
call → primary (finishCall | ".")*
finishCall → "(" (expression ("," expression)*) ")"
primary → "false" | "true" | nil |  literal 
		| "(" expression ")" 
		| super "." identifier ";" 
		| "this" | identifier 
		| lambda 
        | "==" | "!=" | ">=" | "<" | "<=" | "/" | "*" | "-" | "+"   //Error productions
lambda → "fun" "(" (identifier ("," identifier)*)? ")" block
```

Todo: ☐ ☑ ☒

☑ Scanner
 - ☑ Multiline comment support

☑ Tokenizer

☑ Syntax Trees

☑ Parser
 - ☑ Comma operator- Evaluate and throw away expressions
 - ☑ Ternary operator
 - ☑ ++, -- operators

☑ Evaluator
 - ☑ Allow concatenation of strings and numbers

☑ Statements
 - ☑ Support for both expressions and statements

☑ Control Flow
 - ☑ do-while loops
 - ☑ break statement
 - ☑ continue statement

☑ Functions
 - ☑ Lambda functions

☑ Resolving and Binding
 - ☑ Warnings for unused and uninitialized variables

☑ Classes
 - ☑ Static member functions
 - ☑ Getter field methods

☑ Inheritance
 - ☑ Multiple Inheritance

☐ Deploy to a website
 - ☐ Create a Website frontend
 - ☐ Create example programs to show off
 - ☐ Create a Docker container for everything
 - ☐ Deploy to a personal site