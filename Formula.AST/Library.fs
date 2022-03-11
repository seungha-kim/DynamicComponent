namespace Formula.AST

open System.Collections.Generic

type Expression =
    // Literal
    | NumberLit of string
    | TextLit of string

    // Unary Operator
    // Binary Operator
    | AddOp of Expression * Expression
    | MultiplyOp of Expression * Expression
    
    // Function
    | FunctionExpr of string * IEnumerable<Expression>

    // Misc
    | InvalidExpr of string
