namespace FormulaAST

type Expression =
    // Literal
    | NumberLiteral of string
    | TextLiteral of string

    // Binary Operator
    | AddOperator of Expression * Expression
    | MultiplyOperator of Expression * Expression
    
    // Function
    | AbsFunction of Expression
    | AddFunction of Expression * Expression // for test

    // Misc
    | InvalidExpression of string
