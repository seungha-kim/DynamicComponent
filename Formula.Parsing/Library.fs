module Formula.Parsing

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

    let fragIdent =
        identifier (IdentifierOptions(isAsciiIdStart = isAsciiIdStart, isAsciiIdContinue = isAsciiIdContinue)) .>> ws

    // Literals
    let numOpts =
        NumberLiteralOptions.AllowFraction
        ||| NumberLiteralOptions.AllowExponent

    let fragNumber =
        numberLiteral numOpts "number" .>> ws
        |>> fun lit -> Expression.NumberLiteral lit.String

    // Functions
    let fragFunc = pipe2 fragIdent (fragParens (sepBy fragExpr (fragStr ","))) (fun name args -> FunctionExpression(name, args))
    
    // Operator precedence parser

    opp.TermParser <-
        fragNumber
        <|> fragFunc
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
