module Tests

open Xunit
open Formula.AST
open Formula.Parsing
open Formula.TestUtil

let ctx = DummyParsingContext()

let expressionData: obj [] list =
    [ [| "1"; NumberLit "1" |]
      [| "    1"; NumberLit "1" |]
      [| "1    "; NumberLit "1" |]
      [| "111"; NumberLit "111" |]
      [| "(1)"; NumberLit "1" |]

      // Unary operator
      [| "-1"; NegateOp(NumberLit "1") |]
      [| "-     1"; NegateOp(NumberLit "1") |]

      // Binary operator
      [| "1 + 2"
         AddOp(NumberLit "1", NumberLit "2") |]
      [| "1 + 2 + 3"
         AddOp(AddOp(NumberLit "1", NumberLit "2"), NumberLit "3") |]
      [| "1 + 2 * 3"
         AddOp(NumberLit "1", MultiplyOp(NumberLit "2", NumberLit "3")) |]
      [| "(1 + 2) * 3"
         MultiplyOp(AddOp(NumberLit "1", NumberLit "2"), NumberLit "3") |]
      [| "1 - 2"
         SubtractOp(NumberLit "1", NumberLit "2") |]
      [| "1 - -2"
         SubtractOp(NumberLit "1", NegateOp(NumberLit "2")) |]
      [| "- 1 - - 2"
         SubtractOp(NegateOp(NumberLit "1"), NegateOp(NumberLit "2")) |]

      // Function expression
      [| "ABS(1)"
         FunctionExpr("ABS", [ NumberLit "1" ]) |]
      [| "  ABS  (  1  )  "
         FunctionExpr("ABS", [ NumberLit "1" ]) |]
      [| "ADD(1,2)"
         FunctionExpr("ADD", [ NumberLit "1"; NumberLit "2" ]) |]
      [| "  ADD  (  1  ,  2  )  "
         FunctionExpr("ADD", [ NumberLit "1"; NumberLit "2" ]) |]
      [| "ABS(1) + ADD(2, 3)"
         AddOp(FunctionExpr("ABS", [ NumberLit "1" ]), FunctionExpr("ADD", [ NumberLit "2"; NumberLit "3" ])) |] ]

[<Theory; MemberData(nameof (expressionData))>]
let ``Parsing test`` (input: string, expected: Expression) =
    let parser = createParser ()
    let expr = parser.Parse(ctx, input)
    Assert.Equal(expr, expected)
