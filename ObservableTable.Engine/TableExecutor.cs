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
        private readonly HashSet<PropertyDescriptor> _staleProperties;

        public TableExecutor()
        {
            _evaluator = Evaluation.createEvaluator();
            _staleProperties = new HashSet<PropertyDescriptor>();
        }

        internal void Execute(TableExecuteContext context)
        {
            if (context.AnalysisSummary.IsCyclic)
            {
                return;
            }

            foreach (var createdTableId in context.TableModificationSummary.CreatedTableIds)
            {
                var sourceScript = context.ScriptRepository.GetTableScript(createdTableId)!;
                context.RuntimeRepository.CreateTable(sourceScript);
            }

            // TODO: removed properties
            // TODO: removed tables
            // TODO: removed parent?
            InvalidateProperties(context);
            UpdateStaleProperties(context);
            _staleProperties.Clear();
        }

        private void InvalidateProperties(TableExecuteContext context)
        {
            foreach (var desc in context.TableModificationSummary.UpdatedProperties)
            {
                Visit(desc);
            }

            void Visit(PropertyDescriptor desc)
            {
                _staleProperties.Add(desc);
                foreach (var observer in context.AnalysisSummary.GetObservers(desc))
                {
                    Visit(observer);
                }
            }
        }

        private void UpdateStaleProperties(TableExecuteContext context)
        {
            while (_staleProperties.Any())
            {
                Visit(_staleProperties.First());
            }

            void Visit(PropertyDescriptor desc)
            {
                if (!_staleProperties.Contains(desc)) return;

                _staleProperties.Remove(desc);
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
            private readonly TableId _id;
            private readonly TableExecuteContext _executeContext;

            internal EvaluationContext(TableId id, TableExecuteContext executeContext)
            {
                _id = id;
                _executeContext = executeContext;
            }

            public FormulaValue GetIdentifierValue(string name)
            {
                var table = _executeContext.RuntimeRepository.GetTableById(_id);
                if (table is null) return FormulaValue.NullValue;

                var prop = table.GetProperty(name);
                return prop ?? FormulaValue.NullValue;
            }

            public FormulaValue GetPropertyValue(string receiver, string name)
            {
                // TODO: self property?

                var parent = _executeContext.RuntimeRepository.GetParent(_id);
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