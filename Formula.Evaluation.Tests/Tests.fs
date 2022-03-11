module Tests

open Xunit
open Formula.AST
open Formula.Evaluation
open Formula.TestUtil

let ctx = DummyEvaluationContext()


let numberExprData: obj [] list =
    [ [| NumberLit "1"; 1.0f |]
      [| NumberLit "1.0"; 1.0f |]
      [| NumberLit "2.0"; 2.0f |]
      [| NumberLit "-1"; -1.0f |]
      [| NumberLit "-1.0"; -1.0f |]
      [| SubtractOp(NumberLit "1", NumberLit "2")
         -1.0f |]
      [| AddOp(NumberLit "1", NumberLit "2")
         3.0f |]
      [| FunctionExpr("ABS", [ NegateOp(NumberLit "1") ])
         1.0f |]
      [| FunctionExpr("ABS", [ FunctionExpr("ADD", [ NumberLit "-1"; NumberLit "10" ]) ])
         9.0f |] ]

[<Theory; MemberData(nameof (numberExprData))>]
let ``Parsing test`` (input: Expression, expected: float32) =
    let eval = createEvaluator ()
    let result = eval.EvaluateAsNumber(ctx, input)
    Assert.Equal(expected, result)
