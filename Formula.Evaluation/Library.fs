module Formula.Evaluation

open System
open System.Collections.Generic
open System.Linq
open Formula.AST
open Formula.Interface
open Formula.ValueRepresentation
open Formula.Errors

module private argUtils =

    let get1 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)

        if l.Count <> 1 then
            WrongArgumentNumber |> Error
        else
            e.ElementAt(0) |> Ok

    let get2 (e: IEnumerable<Expression>) =
        let l = Enumerable.ToList(e)

        if l.Count <> 2 then
            WrongArgumentNumber |> Error
        else
            (e.ElementAt(0), e.ElementAt(1)) |> Ok

module private funcs =
    let abs (x: float32) : float32 = Math.Abs(x)
    let add (x: float32) (y: float32) : float32 = x + y

module private impl =
    type ResultBuilder() =
        member this.Bind(m, f) = Result.bind f m
        member this.Return(x) = Ok x

    let result = ResultBuilder()

    let rec private evaluate
        (ctx: IEvaluationContext)
        (expr: Expression)
        : Result<FormulaValue, Formula.Errors.Errors> =
        match expr with
        | NumberLit s -> Convert.ToSingle(s) |> NumberValue |> Ok
        | NegateOp operand ->
            result {
                let! x = (evaluate ctx operand)
                let! f = x.asFloat
                return NumberValue f
            }
        | AddOp (lhs, rhs) -> simpleFloatBinaryOp ctx lhs rhs (+)
        | SubtractOp (lhs, rhs) -> simpleFloatBinaryOp ctx lhs rhs (-)
        | MultiplyOp (lhs, rhs) -> simpleFloatBinaryOp ctx lhs rhs (*)
        | FunctionExpr (name, args) -> evaluateFunction ctx name args
        | Ident ident -> ctx.GetIdentifierValue(ident) |> Ok // TODO: Ok 일리가...
        | PropertyExpr (tableName, propertyName) ->
            ctx.GetPropertyValue(tableName, propertyName)
            |> Ok // TODO: Ok 일리가...
        | InvalidExpr _ -> InvalidExpression |> Error

    and evaluateFunction ctx name args =
        match name.ToUpper() with
        | "ABS" ->
            result {
                let! arg = argUtils.get1 args
                let! v = evaluate ctx arg
                let! f = v.asFloat
                return f |> funcs.abs |> NumberValue
            }
        | "ADD" ->
            result {
                let! arg1, arg2 = argUtils.get2 args
                let! v1 = evaluate ctx arg1
                let! f1 = v1.asFloat
                let! v2 = evaluate ctx arg2
                let! f2 = v2.asFloat
                return funcs.add f1 f2 |> NumberValue
            }
        | _ -> Error UnexpectedFunctionName

    and simpleFloatBinaryOp ctx lhs rhs op =
        result {
            let! lv = evaluate ctx lhs
            let! lf = lv.asFloat
            let! rv = evaluate ctx rhs
            let! rf = rv.asFloat
            return op lf rf |> NumberValue
        }

    type FormulaEvaluator() =
        interface IFormulaEvaluator with
            member this.Evaluate(ctx, expr) =
                match (evaluate ctx expr) with
                | Ok v -> v
                | Error e -> raise (Exception("error:" + e.ToString()))

let createEvaluator () =
    impl.FormulaEvaluator() :> IFormulaEvaluator
