module Tests

open Xunit
open FormulaInterface
open FormulaAST

type DummyParsingContext() =
    interface IParsingContext with


let ctx = DummyParsingContext()

let expressionData: obj [] list =
    [ [| "1"; NumberLiteral "1" |]
      [| "-1"; NumberLiteral "-1" |]
      [| "    1"; NumberLiteral "1" |]
      [| "1    "; NumberLiteral "1" |]
      [| "111"; NumberLiteral "111" |]
      [| "(1)"; NumberLiteral "1" |]
      [| "1 + 2"
         AddOperator(NumberLiteral "1", NumberLiteral "2") |]
      [| "1 + 2 + 3"
         AddOperator(AddOperator(NumberLiteral "1", NumberLiteral "2"), NumberLiteral "3") |]
      [| "1 + 2 * 3"
         AddOperator(NumberLiteral "1", MultiplyOperator(NumberLiteral "2", NumberLiteral "3")) |]
      [| "(1 + 2) * 3"
         MultiplyOperator(AddOperator(NumberLiteral "1", NumberLiteral "2"), NumberLiteral "3") |]
      [| "ABS(1)"
         FunctionExpression("ABS", [ NumberLiteral "1" ]) |]
      [| "  ABS  (  1  )  "
         FunctionExpression("ABS", [ NumberLiteral "1" ]) |]
      [| "ADD(1,2)"
         FunctionExpression("ADD", [ NumberLiteral "1"; NumberLiteral "2" ]) |]
      [| "  ADD  (  1  ,  2  )  "
         FunctionExpression("ADD", [ NumberLiteral "1"; NumberLiteral "2" ]) |]
      [| "ABS(1) + ADD(2, 3)"
         AddOperator(
             FunctionExpression("ABS", [ NumberLiteral "1" ]),
             FunctionExpression("ADD", [ NumberLiteral "2"; NumberLiteral "3" ])
         ) |] ]

[<Theory; MemberData(nameof (expressionData))>]
let ``Parsing test`` (input: string, expected: Expression) =
    let parser = FormulaParsing.createParser ()
    let expr = parser.Parse(ctx, input)
    Assert.Equal(expr, expected)
