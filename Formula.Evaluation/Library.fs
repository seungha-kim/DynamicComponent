module Formula.Evaluation

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Formula.AST
open Formula.Interface

module private argUtils =
    let get1 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)
        // todo: assertion and return error
        e.ElementAt(0)

    let get2 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)
        // todo: assertion and return error
        (e.ElementAt(0), e.ElementAt(1))

module private funcs =
    let abs (x: float32) : float32 = Math.Abs(x)
    let add (x: float32) (y: float32) : float32 = x + y

module private impl =
    let rec private evaluateAsNumber ctx expr =
        match expr with
        | NumberLit s -> Convert.ToSingle(s)
        | AddOp (lhs, rhs) ->
            (evaluateAsNumber ctx lhs)
            + (evaluateAsNumber ctx rhs)
        | MultiplyOp (lhs, rhs) ->
            (evaluateAsNumber ctx lhs)
            * (evaluateAsNumber ctx rhs)
        | FunctionExpr (name, args) -> evaluateFunctionAsNumber ctx name args
        | InvalidExpr _ -> 0.0f

    and evaluateFunctionAsNumber ctx name args =
        match name.ToUpper() with
        | "ABS" ->
            let arg = argUtils.get1 args
            funcs.abs (evaluateAsNumber ctx arg)
        | "ADD" ->
            let arg1, arg2 = argUtils.get2 args
            funcs.add (evaluateAsNumber ctx arg1) (evaluateAsNumber ctx arg2)
        | _ -> 0.0f // TODO: throw?

    let rec private evaluateAsVoid ctx expr =
        match expr with
        | _ -> ()

    type FormulaEvaluator() =
        interface IFormulaEvaluator with
            member this.EvaluateAsNumber(ctx, expr) = evaluateAsNumber ctx expr

            member this.EvaluateAsVoid(ctx, expr) = evaluateAsVoid ctx expr

let createEvaluator () =
    impl.FormulaEvaluator() :> IFormulaEvaluator
