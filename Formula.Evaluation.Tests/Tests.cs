using System;
using System.Collections.Generic;
using Formula.AST;
using static Formula.AST.Expression;
using Formula.Interface;
using Microsoft.FSharp.Collections;
using Xunit;
using Formula.TestUtil;

namespace Formulas.Evaluation.Tests
{
    public class Tests
    {
        [Theory, MemberData(nameof(ExpressionData))]
        public static void TestEvaluationAsNumber(Expression input, float expected)
        {
            var ctx = new DummyEvaluationContext();
            var eval = Formula.Evaluation.createEvaluator();
            Assert.Equal(expected, eval.EvaluateAsNumber(ctx, input));
        }

        public static IEnumerable<object[]> ExpressionData()
        {
            return new[]
            {
                new object[] {NewNumberLiteral("1"), 1.0f},
                new object[] {NewNumberLiteral("1.0"), 1.0f},
                new object[] {NewNumberLiteral("2.0"), 2.0f},
                new object[] {NewNumberLiteral("-1"), -1.0f},
                new object[] {NewNumberLiteral("-1.0"), -1.0f},
                new object[]
                {
                    NewFunctionExpression("ADD", new[]
                    {
                        NewNumberLiteral("1"),
                        NewNumberLiteral("2"),
                    }),
                    3.0f
                },
                new object[]
                {
                    NewFunctionExpression("ABS", new[]
                    {
                        NewNumberLiteral("-1"),
                    }),
                    1.0f
                },
                // multiple functions
                new object[]
                {
                    NewFunctionExpression("ABS", new[]
                    {
                        NewFunctionExpression("ADD", new[]
                        {
                            NewNumberLiteral("-1"),
                            NewNumberLiteral("10"),
                        })
                    }),
                    9.0f
                },
            };
        }
    }
}