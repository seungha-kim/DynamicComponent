using System;
using System.Collections.Generic;
using Xunit;
using Formula.TestUtil;
using Formula.ValueRepresentation;
using static Formula.ValueRepresentation.FormulaValue;
using Xunit.Abstractions;

namespace Formula.Tests
{
    public class Tests
    {
        private readonly ITestOutputHelper _out;

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _out = testOutputHelper;
        }

        [Theory, MemberData(nameof(ExpressionData))]
        public void TestEvaluate(string input, FormulaValue expected)
        {
            var pCtx = new DummyParsingContext();
            var parser = Formula.Parsing.createParser();
            var expr = parser.Parse(pCtx, input);
            var eCtx = new DummyEvaluationContext();
            var eval = Formula.Evaluation.createEvaluator();
            _out.WriteLine($"input: {input}");
            _out.WriteLine($"expr: {expr}");
            Assert.Equal(expected, eval.Evaluate(eCtx, expr));
        }

        public static IEnumerable<object[]> ExpressionData()
        {
            return new[]
            {
                new object[] {"1", NewNumberValue(1.0f)},
                new object[] {"ADD(1, 2)", NewNumberValue(3.0f)},
                new object[] {"ABS(1)", NewNumberValue(1.0f)},
                new object[] {"ABS(-1)", NewNumberValue(1.0f)},
            };
        }
    }
}