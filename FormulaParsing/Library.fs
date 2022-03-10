module FormulaParsing

open FParsec
open FormulaAST
open FormulaInterface

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

    let fragFuncName name = fragStrCI name |>> fun s -> s.ToUpper()
    // Literals
    let numOpts =
        NumberLiteralOptions.AllowFraction
        ||| NumberLiteralOptions.AllowExponent

    let fragNumber =
        numberLiteral numOpts "number" .>> ws
        |>> fun lit -> Expression.NumberLiteral lit.String

    // Built-in functions
    let getFunc1 name arg =
        match name with
        | "ABS" -> AbsFunction arg
        | _ -> failwith "todo"

    let getFunc2 name arg1 arg2 =
        match name with
        | "ADD" -> AddFunction(arg1, arg2)
        | _ -> failwith "todo"

    let fragFunc1 name =
        pipe2 (fragFuncName name .>> fragStr "(") (fragExpr .>> fragStr ")") getFunc1

    let fragFunc2 name =
        pipe3 (fragFuncName name .>> fragStr "(") (fragExpr .>> fragStr ",") (fragExpr .>> fragStr ")") getFunc2

    let fragAbsFunction = fragFunc1 "ABS"
    let fragAddFunction = fragFunc2 "ADD"

    let allFunctions =
        choice [ fragAbsFunction
                 fragAddFunction ]

    // Operator precedence parser

    opp.TermParser <-
        fragNumber
        <|> between (fragStr "(") (fragStr ")") fragExpr

    opp.AddOperator(InfixOperator("+", ws, 10, Associativity.Left, (fun x y -> AddOperator(x, y))))
    opp.AddOperator(InfixOperator("*", ws, 20, Associativity.Left, (fun x y -> MultiplyOperator(x, y))))

    let pExpression = ws >>. fragExpr .>> eof

    type FormulaParser() =
        interface IFormulaParser with
            member this.Parse(ctx, input) =
                match run pExpression input with
                | Success (expression, unit, position) -> expression
                | Failure (s, parserError, unit) -> InvalidExpression(parserError.Messages.ToString())

let createParser () = impl.FormulaParser() :> IFormulaParser
