namespace Formula.AST

open System.Collections.Generic
open Formula.AST

type Expression =
    // Literal
    | NumberLit of string
    | TextLit of string

    // Identifier
    | Ident of string

    // Property
    | PropertyExpr of string * string

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

    member this.GetInnerReferences() : IEnumerable<Expression> =
        let result = List<Expression>()

        let rec traverse expr =
            match expr with
            | Ident _ -> result.Add(expr)
            | PropertyExpr _ -> result.Add(expr)

            | NegateOp e -> traverse e

            | AddOp (e1, e2)
            | SubtractOp (e1, e2)
            | MultiplyOp (e1, e2) ->
                traverse e1
                traverse e2

            | FunctionExpr (_, es) ->
                for e in es do
                    traverse e

            | _ -> ()

        result
