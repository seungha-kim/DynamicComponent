module Tests

open Formula.ValueRepresentation
open Xunit
open Formula.AST
open Formula.Evaluation
open Formula.TestUtil

let ctx = DummyEvaluationContext()


let numberExprData: obj [] list =
    [ [| NumberLit "1"; NumberValue 1.0f |]
      [| NumberLit "1.0"; NumberValue 1.0f |]
      [| NumberLit "2.0"; NumberValue 2.0f |]
      [| NumberLit "-1"; NumberValue -1.0f |]
      [| NumberLit "-1.0"
         NumberValue -1.0f |]
      [| SubtractOp(NumberLit "1", NumberLit "2")
         NumberValue -1.0f |]
      [| AddOp(NumberLit "1", NumberLit "2")
         NumberValue 3.0f |]
      [| FunctionExpr("ABS", [ NegateOp(NumberLit "1") ])
         NumberValue 1.0f |]
      [| FunctionExpr("ABS", [ FunctionExpr("ADD", [ NumberLit "-1"; NumberLit "10" ]) ])
         NumberValue 9.0f |] ]

[<Theory; MemberData(nameof (numberExprData))>]
let ``Parsing test`` (input: Expression, expected: FormulaValue) =
    let eval = createEvaluator ()
    let result = eval.Evaluate(ctx, input)
    Assert.Equal(expected, result)
