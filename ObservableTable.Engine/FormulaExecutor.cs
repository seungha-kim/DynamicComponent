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
            var result = Evaluator.Evaluate(_evaluationContext, expr);
            table.UpdateProperty(propertyName, result);
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