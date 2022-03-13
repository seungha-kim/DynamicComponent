using Formula.Interface;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    internal class FormulaExecutor
    {
        private IFormulaParser Parser { get; } = Formula.Parsing.createParser();
        private IFormulaEvaluator Evaluator { get; } = Formula.Evaluation.createEvaluator();
        private ITableRepository Repo { get; }
        private readonly IParsingContext _parsingContext = new ParsingContext();

        internal FormulaExecutor(ITableRepository repo)
        {
            Repo = repo;
        }

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
            var ctx = new EvaluationContext(table.ID, Repo);
            var result = Evaluator.Evaluate(ctx, expr);
            table.UpdateProperty(propertyName, result);
        }

        private class ParsingContext : IParsingContext
        {
        }

        private class EvaluationContext : IEvaluationContext
        {
            private TableId ID { get; }
            private ITableRepository TableRepository { get; }

            internal EvaluationContext(TableId id, ITableRepository tableRepository)
            {
                ID = id;
                TableRepository = tableRepository;
            }

            public FormulaValue LookupIdentifier(string name)
            {
                var table = TableRepository.GetTableById(ID);
                if (table is null) return FormulaValue.NullValue;

                var prop = table.GetProperty(name);
                return prop ?? FormulaValue.NullValue;
            }

            public FormulaValue LookupProperty(string receiver, string name)
            {
                // TODO: self property?

                var parent = TableRepository.GetParent(ID);
                if (parent is null) return FormulaValue.NullValue;
                if (parent.Name == receiver)
                {
                    var parentProp = parent.GetProperty(name);
                    if (parentProp is { }) return parentProp;
                }

                foreach (var sibling in TableRepository.GetChildrenById(parent.ID))
                {
                    if (sibling.Name != receiver) continue;

                    var siblingProp = sibling.GetProperty(name);
                    if (siblingProp is { }) return siblingProp;
                }

                return FormulaValue.NullValue;
            }
        }
    }
}