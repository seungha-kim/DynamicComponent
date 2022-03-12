module Formula.Evaluation

open System
open System.Collections.Generic
open System.Linq
open Formula.AST
open Formula.Interface
open Formula.ValueRepresentation

module Exceptions =
    // TODO: more info
    exception UnexpectedTypeError
    exception UnexpectedFunctionNameError
    exception InvalidExpressionError
    exception WrongArgumentNumberError

module private argUtils =
    open Exceptions

    let get1 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)

        if l.Count <> 1 then
            raise WrongArgumentNumberError

        e.ElementAt(0)

    let get2 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)

        if l.Count <> 2 then
            raise WrongArgumentNumberError

        (e.ElementAt(0), e.ElementAt(1))

module private funcs =
    let abs (x: float32) : float32 = Math.Abs(x)
    let add (x: float32) (y: float32) : float32 = x + y

module private impl =
    open Exceptions

    let asFloat (value: FormulaValue) : float32 =
        match value with
        | NumberValue f -> f
        | _ -> raise UnexpectedTypeError

    let rec private evaluate ctx expr =
        match expr with
        | NumberLit s -> Convert.ToSingle(s) |> NumberValue
        | NegateOp operand ->
            (evaluate ctx operand |> asFloat) * -1.0f
            |> NumberValue
        | AddOp (lhs, rhs) ->
            (evaluate ctx lhs |> asFloat)
            + (evaluate ctx rhs |> asFloat)
            |> NumberValue
        | SubtractOp (lhs, rhs) ->
            NumberValue(
                (evaluate ctx lhs |> asFloat)
                - (evaluate ctx rhs |> asFloat)
            )
        | MultiplyOp (lhs, rhs) ->
            (evaluate ctx lhs |> asFloat)
            * (evaluate ctx rhs |> asFloat)
            |> NumberValue
        | FunctionExpr (name, args) -> evaluateFunction ctx name args
        | InvalidExpr _ -> raise InvalidExpressionError

    and evaluateFunction ctx name args =
        match name.ToUpper() with
        | "ABS" ->
            let arg = argUtils.get1 args

            evaluate ctx arg
            |> asFloat
            |> funcs.abs
            |> NumberValue
        | "ADD" ->
            let arg1, arg2 = argUtils.get2 args

            (evaluate ctx arg1 |> asFloat, evaluate ctx arg2 |> asFloat)
            ||> funcs.add
            |> NumberValue
        | _ -> raise UnexpectedFunctionNameError

    type FormulaEvaluator() =
        interface IFormulaEvaluator with
            member this.Evaluate(ctx, expr) = evaluate ctx expr

let createEvaluator () =
    impl.FormulaEvaluator() :> IFormulaEvaluator
