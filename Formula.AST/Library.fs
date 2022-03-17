namespace Formula.AST

open System
open System.Collections.Generic

type Expression =
    // Literal
    | NumberLit of string
    | TextLit of string

    // Identifier
    | Ident of string

    // Property: identifier.propertyName
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

        result :> IEnumerable<Expression>

    member this.AsIdentifierName() : string =
        match this with
        | Ident s -> s
        | _ -> raise (Exception("TODO: Invalid casting"))


    member this.AsProperty() : string * string =
        match this with
        | PropertyExpr (ident, prop) -> (ident, prop)
        | _ -> raise (Exception("TODO: Invalid casting"))
