using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.Interface;

namespace ObservableTable.Engine
{
    internal class TableAnalyzer
    {
        // Components
        private readonly IFormulaParser _parser;
        private readonly ParsingContext _parsingContext;

        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _observers;
        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _references;
        private List<PropertyDescriptor>? Cycle { get; set; }

        internal TableAnalyzer()
        {
            _parser = Parsing.createParser();
            _parsingContext = new ParsingContext();
            _observers = new Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>>();
            _references = new Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>>();
        }

        internal void Update(TableAnalyzeContext context)
        {
            UpdateExpressions(context);
            InvalidateReferences(context);
            UpdateReferences(context);
        }

        internal ITableAnalysisSummary GetSummary()
        {
            return new TableAnalysisSummary(this);
        }

        private void UpdateExpressions(TableAnalyzeContext context)
        {
            foreach (var removedDesc in context.TableModificationSummary.RemovedProperties)
            {
                context.PropertyExpressionRepository.RemovePropertyExpression(removedDesc);
            }

            foreach (var updatedDesc in context.TableModificationSummary.UpdatedProperties)
            {
                var formula =
                    context.ScriptRepository.GetTableScript(updatedDesc.ID)!.GetPropertyFormula(updatedDesc.Name)!;
                var expr = _parser.Parse(_parsingContext, formula);
                // TODO: 에러 처리 - 애초에 Parse 가 Result 를 반환해야 될 것 같은데
                context.PropertyExpressionRepository.SetPropertyExpression(updatedDesc, expr);
            }
        }

        private void InvalidateReferences(TableAnalyzeContext context)
        {
            var summary = context.TableModificationSummary;
            var invalidation = summary.RemovedProperties.Concat(summary.UpdatedProperties);
            foreach (var desc in invalidation)
            {
                if (!_references.ContainsKey(desc)) continue;

                var references = _references[desc]!;
                _references.Remove(desc);

                foreach (var reference in references)
                {
                    var observers = _observers[reference]!;
                    observers.Remove(desc);
                    if (!observers.Any()) _observers.Remove(reference);
                }
            }
        }

        private void UpdateReferences(TableAnalyzeContext context)
        {
            var propertyExpressionRepository = context.PropertyExpressionRepository;
            var tableScriptRepository = context.ScriptRepository;

            foreach (var desc in context.TableModificationSummary.UpdatedProperties)
            {
                var expr = propertyExpressionRepository.GetPropertyExpression(desc);
                foreach (var reference in expr.GetInnerReferences())
                {
                    if (reference.IsIdent)
                    {
                        var propertyName = reference.AsIdentifierName()!;
                        if (tableScriptRepository.GetSelfPropertyOfReference(desc.ID, propertyName) is
                            { } referenceDesc)
                            AddReference(desc, referenceDesc);
                    }

                    if (reference.IsPropertyExpr)
                    {
                        var (tableName, propertyName) = reference.AsProperty();
                        if (tableScriptRepository.GetScopedPropertyOfReference(desc.ID, tableName, propertyName) is
                            { } referenceDesc)
                            AddReference(desc, referenceDesc);
                    }
                }
            }
        }

        private void AddReference(PropertyDescriptor desc, PropertyDescriptor referenceDesc)
        {
            if (!_references.ContainsKey(desc)) _references[desc] = new HashSet<PropertyDescriptor>();
            _references[desc].Add(referenceDesc);

            if (!_observers.ContainsKey(referenceDesc))
                _observers[referenceDesc] = new HashSet<PropertyDescriptor>();
            _observers[referenceDesc].Add(desc);
        }

        private class ParsingContext : IParsingContext
        {
        }

        private class TableAnalysisSummary : ITableAnalysisSummary
        {
            private TableAnalyzer _analyzer;

            internal TableAnalysisSummary(TableAnalyzer analyzer)
            {
                _analyzer = analyzer;
            }

            IEnumerable<PropertyDescriptor> ITableAnalysisSummary.GetReferences(PropertyDescriptor desc)
            {
                if (!_analyzer._references.TryGetValue(desc, out var references))
                    yield break;
                foreach (var r in references) yield return r;
            }

            IEnumerable<PropertyDescriptor> ITableAnalysisSummary.GetObservers(PropertyDescriptor desc)
            {
                if (!_analyzer._observers.TryGetValue(desc, out var observers)) yield break;
                foreach (var r in observers) yield return r;
            }

            bool ITableAnalysisSummary.IsCyclic => _analyzer.Cycle is { };

            IEnumerable<PropertyDescriptor>? ITableAnalysisSummary.GetReferenceCycle()
            {
                return _analyzer.Cycle;
            }

            bool ITableAnalysisSummary.IsPropertyInCycle(PropertyDescriptor desc)
            {
                return _analyzer.Cycle?.Contains(desc) ?? false;
            }
        }
    }
}