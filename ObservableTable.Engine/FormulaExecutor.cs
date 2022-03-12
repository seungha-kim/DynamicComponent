using System;
using System.Collections.Generic;
using Formula.AST;
using Formula.Interface;

namespace ObservableTable.Engine
{
    internal class FormulaExecutor
    {
        private IFormulaParser Parser { get; } = Formula.Parsing.createParser();
        private IFormulaEvaluator Evaluator { get; } = Formula.Evaluation.createEvaluator();
        private readonly IParsingContext _parsingContext = new ParsingContext();
        private readonly IEvaluationContext _evaluationContext = new EvaluationContext();

        internal void UpdateProperty(TableScript script, Table table, string propertyName)
        {
            var formula = script.GetPropertyFormula(propertyName);
            if (formula is null) return;

            var desc = new PropertyDescriptor(table.ID, propertyName);
            if (!script.ExpressionCache.ContainsKey(propertyName))
            {
                var cache = Parser.Parse(_parsingContext, formula);
                script.ExpressionCache[propertyName] = cache;
            }

            var expr = script.ExpressionCache[propertyName];
            // TODO: 정해져있는 타입이 있으면 해당 타입으로 평가하기. 간단한 타입추론을 하거나, 동적 타입시스템으로 바꿔야 할 수도 (값이 타입을 갖게 만들기)
            var result = Evaluator.EvaluateAsNumber(_evaluationContext, expr);
            table.UpdateNumberProperty(propertyName, result);
        }

        private class ParsingContext : IParsingContext
        {
        }

        private class EvaluationContext : IEvaluationContext
        {
            // TODO: identifier lookup
        }
    }
}