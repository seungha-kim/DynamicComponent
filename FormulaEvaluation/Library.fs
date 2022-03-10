module FormulaEvaluation

open System
open FormulaAST
open FormulaInterface

module private impl =
    let rec private evaluateAsNumber ctx expr =
        match expr with
        | NumberLiteral s -> Convert.ToSingle(s)
        | AddOperator (lhs, rhs) ->
            (evaluateAsNumber ctx lhs)
            + (evaluateAsNumber ctx rhs)
        | MultiplyOperator (lhs, rhs) ->
            (evaluateAsNumber ctx lhs)
            * (evaluateAsNumber ctx rhs)
        | InvalidExpression _ -> 0.0f

    let rec private evaluateAsVoid ctx expr =
        match expr with
        | _ -> ()

    type FormulaEvaluator() =
        interface IFormulaEvaluator with
            member this.EvaluateAsNumber(ctx, expr) = evaluateAsNumber ctx expr

            member this.EvaluateAsVoid(ctx, expr) = evaluateAsVoid ctx expr

let createEvaluator () = impl.FormulaEvaluator() :> IFormulaEvaluator