using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.Interface;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    internal class TableExecutor
    {
        private readonly IFormulaEvaluator _evaluator;
        private readonly HashSet<PropertyDescriptor> _notVisited;

        public TableExecutor()
        {
            _evaluator = Evaluation.createEvaluator();
            _notVisited = new HashSet<PropertyDescriptor>();
        }

        internal void Execute(TableExecuteContext context)
        {
            if (context.AnalysisSummary.IsCyclic)
            {
                throw new Exception("TODO: cyclic");
            }

            foreach (var createdTableId in context.TableModificationSummary.CreatedTableIds)
            {
                var sourceScript = context.ScriptRepository.GetTableScript(createdTableId)!;
                context.RuntimeRepository.CreateTable(sourceScript);
            }

            EvaluateProperties(context);
        }

        private void EvaluateProperties(TableExecuteContext context)
        {
            // TODO: removed properties
            // TODO: removed tables
            // TODO: removed parent?
            _notVisited.Clear();
            foreach (var desc in context.TableModificationSummary.UpdatedProperties)
            {
                _notVisited.Add(desc);
            }

            while (_notVisited.Any())
            {
                Visit(_notVisited.First());
            }
            // TODO: 문제 있음 - observer 는 업데이트가 되지 않음. 업데이트 되어야 하는 속성 마킹하는 단계 추가

            void Visit(PropertyDescriptor desc)
            {
                if (!_notVisited.Contains(desc)) return;

                _notVisited.Remove(desc);
                foreach (var reference in context.AnalysisSummary.GetReferences(desc))
                {
                    Visit(reference);
                }

                var expr = context.PropertyExpressionRepository.GetPropertyExpression(desc)!;
                var evaluationContext = new EvaluationContext(desc.ID, context);
                var value = _evaluator.Evaluate(evaluationContext, expr);
                var table = context.RuntimeRepository.GetTableById(desc.ID)!;
                table.UpdateProperty(desc.Name, value);
            }
        }

        private class EvaluationContext : IEvaluationContext
        {
            private TableId ID { get; }
            private TableExecuteContext _executeContext;

            internal EvaluationContext(TableId id, TableExecuteContext executeContext)
            {
                ID = id;
                _executeContext = executeContext;
            }

            public FormulaValue GetIdentifierValue(string name)
            {
                var table = _executeContext.RuntimeRepository.GetTableById(ID);
                if (table is null) return FormulaValue.NullValue;

                var prop = table.GetProperty(name);
                return prop ?? FormulaValue.NullValue;
            }

            public FormulaValue GetPropertyValue(string receiver, string name)
            {
                // TODO: self property?

                var parent = _executeContext.RuntimeRepository.GetParent(ID);
                if (parent is null) return FormulaValue.NullValue;
                if (parent.Name == receiver)
                {
                    var parentProp = parent.GetProperty(name);
                    if (parentProp is { }) return parentProp;
                }


                foreach (var siblingScript in _executeContext.ScriptRepository.GetChildren(parent.ID))
                {
                    if (siblingScript.Name != receiver) continue;

                    var siblingRuntime = _executeContext.RuntimeRepository.GetTableById(siblingScript.ID)!;
                    var siblingProp = siblingRuntime.GetProperty(name);
                    if (siblingProp is { }) return siblingProp;
                }

                return FormulaValue.NullValue;
            }
        }
    }
}