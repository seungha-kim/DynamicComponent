﻿module Formula.Parsing

open FParsec
open Formula.AST
open Formula.Interface

module private impl =
    // https://www.quanttec.com/fparsec/users-guide/tips-and-tricks.html
    // https://github.com/stephan-tolksdorf/fparsec/blob/master/Samples/Calculator/calculator.fs
    // https://rosalogia.me/posts/functional-parsing/#statements

    // Primitives
    let opp =
        OperatorPrecedenceParser<Expression, unit, unit>()

    let fragExpr = opp.ExpressionParser
    let ws = spaces
    let fragStr s = pstring s .>> ws
    let fragStrCI s = pstringCI s .>> ws

    let fragParens p =
        between (fragStr "(") (fragStr ")") p .>> ws

    let isAsciiIdStart c = isAsciiLetter c || c = '_'

    let isAsciiIdContinue c =
        isAsciiLetter c
        || isDigit c
        || c = '_'
        || c = '\''

    let identOpts =
        IdentifierOptions(isAsciiIdStart = isAsciiIdStart, isAsciiIdContinue = isAsciiIdContinue)

    let fragIdentStr = identifier identOpts .>> ws

    // Literals
    let numOpts =
        NumberLiteralOptions.AllowFraction
        ||| NumberLiteralOptions.AllowExponent

    let fragNumber =
        numberLiteral numOpts "number" .>> ws
        |>> fun lit -> Expression.NumberLit lit.String

    // Functions
    let fragFunc =
        pipe2 fragIdentStr (fragParens (sepBy fragExpr (fragStr ","))) (fun name args -> FunctionExpr(name, args))

    // Property
    let fragProp =
        pipe2 (fragIdentStr .>> (fragStr "!")) fragIdentStr (fun receiver prop -> PropertyExpr(receiver, prop))

    // Identifier
    let fragIdent = fragIdentStr |>> Ident

    // Operator precedence parser
    opp.TermParser <-
        fragNumber
        <|> between (fragStr "(") (fragStr ")") fragExpr
        <|> attempt fragFunc
        <|> attempt fragProp
        <|> fragIdent

    opp.AddOperator(InfixOperator("+", ws, 10, Associativity.Left, (fun x y -> AddOp(x, y))))
    opp.AddOperator(InfixOperator("-", ws, 10, Associativity.Left, (fun x y -> SubtractOp(x, y))))
    opp.AddOperator(InfixOperator("*", ws, 20, Associativity.Left, (fun x y -> MultiplyOp(x, y))))
    opp.AddOperator(PrefixOperator("-", ws, 30, true, (fun x -> NegateOp x)))

    let pExpression = ws >>. fragExpr .>> eof

    type FormulaParser() =
        interface IFormulaParser with
            member this.Parse(ctx, input) =
                match run pExpression input with
                | Success (expression, unit, position) -> expression
                | Failure (s, parserError, unit) -> InvalidExpr(parserError.Messages.ToString())

let createParser () = impl.FormulaParser() :> IFormulaParser
