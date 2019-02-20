## SimpleExpressions

#### Parsing, building and optimizing Expression trees demo

### SameMethodsCalls (memoization)
* Eg.: (int x, int y) => F(x) > F(y) ? F(x) : (F(x) < F(2 * y) ? F(2 * y) : F(y))
* Substitutes repeating calls of expensive methods with condition expression wich places variable instead of method call and assign it during run-time only if neccessary
* Single responsibility
* Implements Visitor behavior while processing expression nodes
* Thread-safe
