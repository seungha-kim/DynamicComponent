using System;
using FormulaAST;
using static FormulaAST.Expression;
using FormulaInterface;
using Xunit;

namespace FormulaEvaluatorTest
{
    internal class DummyEvaluationContext : IEvaluationContext
    {
    }

    public class Tests
    {
        [Fact]
        public void TestSomeOperatorsTodo()
        {
            var ctx = new DummyEvaluationContext();
            var expr = NewMultiplyOperator(
                NewAddOperator(NewNumberLiteral("1"), NewNumberLiteral("2")),
                NewNumberLiteral("3")
                );
            IFormulaEvaluator eval = FormulaEvaluation.createEvaluator();
            Assert.Equal(eval.EvaluateAsNumber(ctx, expr), 9F);
        }
    }
}
