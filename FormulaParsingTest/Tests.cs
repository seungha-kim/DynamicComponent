using System;
using System.Collections.Generic;
using FormulaAST;
using static FormulaAST.Expression;
using Xunit;
using FormulaInterface;

namespace FormulaParserTest
{
    internal class DummyParsingContext : IParsingContext
    {
    }

    public static class Tests
    {
        [Theory, MemberData(nameof(ExpressionData))]
        public static void TestExpression(string input, Expression expected)
        {
            var ctx = new DummyParsingContext();
            var parser = FormulaParsing.createParser();;
            var expr = parser.Parse(ctx, input);
            Assert.Equal(expected, expr);
        }

        public static IEnumerable<object[]> ExpressionData()
        {
            return new[]
            {
                new object[] {"1", NewNumberLiteral("1")},
                new object[] {"  1", NewNumberLiteral("1")},
                new object[] {"1  ", NewNumberLiteral("1")},
                new object[] {"111", NewNumberLiteral("111")},
                new object[] {"(1)", NewNumberLiteral("1")},
                new object[] {"1+2", NewAddOperator(NewNumberLiteral("1"), NewNumberLiteral("2"))},
                new object[] {"1 + 2", NewAddOperator(NewNumberLiteral("1"), NewNumberLiteral("2"))},
                new object[]
                {
                    "1 + 2 * 3",
                    NewAddOperator(
                        NewNumberLiteral("1"),
                        NewMultiplyOperator(NewNumberLiteral("2"), NewNumberLiteral("3"))
                    )
                },
                new object[]
                {
                    "(1 + 2) * 3",
                    NewMultiplyOperator(
                        NewAddOperator(NewNumberLiteral("1"), NewNumberLiteral("2")),
                        NewNumberLiteral("3")
                    )
                },
                new object[]
                {
                    "ABS(1)",
                    NewAbsFunction(NewNumberLiteral("1"))
                },
            };
        }
    }
}