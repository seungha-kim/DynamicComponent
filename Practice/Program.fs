module Program

open FParsec

let __section (str: string) = printfn "\n***** SECTION: %s *****" str

let aaas = dict [(1, 2)]

let test p str =
    match run p str with
    | Success (result, _, _) -> printfn "Success: %A" result
    | Failure (errorMsg, _, _) -> printfn "Failure: %s" errorMsg

let between pBegin pEnd p = pBegin >>. p .>> pEnd
let betweenString s1 s2 p = between (pstring s1) (pstring s2) p

type ASDF =
    { name: string; age: int }
    member private this.HelloPrivate = printfn "Hello %s" this.name
    member this.Hello = this.HelloPrivate

type Choices =
    | Choice1
    | Choice2

    member this.Print =
        match this with
        | Choice1 -> printfn "Choice1"
        | Choice2 -> printfn "Choice2"

module OPP =
    let ws  = spaces  // whitespace parser
    type Expr =
          | InfixOpExpr of string * Expr * Expr
          | Number of int

    let isSymbolicOperatorChar = isAnyOf "!%&*+-./<=>@^|~?"
    let remainingOpChars_ws = manySatisfy isSymbolicOperatorChar .>> ws

    let opp = new OperatorPrecedenceParser<Expr, string, unit>()
    opp.TermParser <- pint32 .>> ws |>> Number

    // a helper function for adding infix operators to opp
    let addSymbolicInfixOperators prefix precedence associativity =
        let op = InfixOperator(prefix, remainingOpChars_ws,
                               precedence, associativity, (),
                               fun remOpChars expr1 expr2 ->
                                   InfixOpExpr(prefix + remOpChars, expr1, expr2))
        opp.AddOperator(op)

    // the operator definitions:
    addSymbolicInfixOperators "*"  10 Associativity.Left
    addSymbolicInfixOperators "**" 20 Associativity.Right

[<EntryPoint>]
let main argv =
    let asdf: ASDF = { name = "seungha"; age = 1 }
    let cccc: Choices = Choice1
    let zxcv = Choices.Choice1
    zxcv.Print
    cccc.Print
    asdf.Hello

    test pfloat "1.1"

    __section "abstraction, custom error"

    let floatBetweenBrackets =
        betweenString "[" "]" (pfloat <?> "부동소수점이 와야 합니다")

    test floatBetweenBrackets "[1.1]"

    __section "many"
    test (many floatBetweenBrackets) "[1][2][3.1][asdf]"

    __section "sepBy, spaces"

    let pFloatList =
        sepBy pfloat (spaces >>. pstring "," .>> spaces)
        |> betweenString "[" "]"

    test pFloatList "[1,2,3]"
    test pFloatList "[1 ,2, 3]"
    test pFloatList "[]"
    test pFloatList "[1][2]"


    __section "eof"
    let pFloatListFile = pFloatList .>> eof
    test pFloatListFile "[1]"
    test pFloatListFile "[1]   "
    test pFloatListFile "[1][2]"

    __section "many1Satisfy2L"

    let pIdentifier =
        let isIdentFirstChar c = isLetter c || c = '_'
        let isIdentChar c = isLetter c || isDigit c || c = '_'

        many1Satisfy2L isIdentFirstChar isIdentChar "identifier"
        .>> spaces

    test pIdentifier "1"
    test pIdentifier "hello"
    test pIdentifier "_"
    test pIdentifier "1hello_"
    test pIdentifier "1_hello"
    test pIdentifier "hello_1"
    test pIdentifier "hello1_"
    test pIdentifier "_hello1"
    test pIdentifier "_1hello"

    __section "stringLiteral"

    let stringLiteral =
        let normalChar = satisfy (fun c -> c <> '\\' && c <> '"')

        let unescape c =
            match c with
            | 'n' -> '\n'
            | 'r' -> '\r'
            | 't' -> '\t'
            | c -> c

        let escapedChar =
            pstring "\\" >>. (anyOf "\\nrt\"" |>> unescape)

        between (pstring "\"") (pstring "\"") (manyChars (normalChar <|> escapedChar))

    let stringLiteral2 =
        let normalCharSnippet =
            many1Satisfy (fun c -> c <> '\\' && c <> '"')

        let escapedChar =
            pstring "\\"
            >>. (anyOf "\\nrt\""
                 |>> function
                     | 'n' -> "\n"
                     | 'r' -> "\r"
                     | 't' -> "\t"
                     | c -> string c)

        between (pstring "\"") (pstring "\"") (manyStrings (normalCharSnippet <|> escapedChar))

    let stringLiteral3 =
        let normalCharSnippet =
            manySatisfy (fun c -> c <> '\\' && c <> '"')

        let escapedChar =
            pstring "\\"
            >>. (anyOf "\\nrt\""
                 |>> function
                     | 'n' -> "\n"
                     | 'r' -> "\r"
                     | 't' -> "\t"
                     | c -> string c)

        between (pstring "\"") (pstring "\"") (stringsSepBy normalCharSnippet escapedChar)

    let pppp = pstring "test"

    __section "OPP"
    test OPP.opp.ExpressionParser "1*!!!!!!!!!2*.3**4**.5"
    
    0

