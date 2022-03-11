namespace FormulaAST

open System.Collections.Generic

type Expression =
    // Literal
    | NumberLiteral of string
    | TextLiteral of string

    // Binary Operator
    | AddOperator of Expression * Expression
    | MultiplyOperator of Expression * Expression
    
    // Function
    | FunctionExpression of string * IEnumerable<Expression>

    // Misc
    | InvalidExpression of string
