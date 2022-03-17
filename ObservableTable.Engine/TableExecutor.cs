using Formula;
using Formula.Interface;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    internal class TableExecutor
    {
        private readonly IFormulaEvaluator _evaluator;

        public TableExecutor()
        {
            _evaluator = Evaluation.createEvaluator();
        }

        internal void Execute(TableExecuteContext context)
        {
        }

        private class EvaluationContext : IEvaluationContext
        {
            internal EvaluationContext(TableId id, ITableRuntimeReadable tableRuntimeReadable)
            {
                ID = id;
                TableRuntimeReadable = tableRuntimeReadable;
            }

            private TableId ID { get; }
            private ITableRuntimeReadable TableRuntimeReadable { get; }

            public FormulaValue GetIdentifierValue(string name)
            {
                var table = TableRuntimeReadable.GetTableById(ID);
                if (table is null) return FormulaValue.NullValue;

                var prop = table.GetProperty(name);
                return prop ?? FormulaValue.NullValue;
            }

            public FormulaValue GetPropertyValue(string receiver, string name)
            {
                // TODO: self property?

                var parent = TableRuntimeReadable.GetParent(ID);
                if (parent is null) return FormulaValue.NullValue;
                if (parent.Name == receiver)
                {
                    var parentProp = parent.GetProperty(name);
                    if (parentProp is { }) return parentProp;
                }

                foreach (var sibling in TableRuntimeReadable.GetChildrenById(parent.ID))
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