namespace Formula.AST

open System.Collections.Generic

type Expression =
    // Literal
    | NumberLit of string
    | TextLit of string

    // Unary Operator
    | NegateOp of Expression

    // Binary Operator
    | AddOp of Expression * Expression
    | SubtractOp of Expression * Expression
    | MultiplyOp of Expression * Expression
    
    // Function
    | FunctionExpr of string * IEnumerable<Expression>

    // Misc
    | InvalidExpr of string
