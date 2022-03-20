using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.AST;
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
        private IEnumerable<PropertyDescriptor>? _cycle;

        internal TableAnalyzer()
        {
            _parser = Parsing.createParser();
            _parsingContext = new ParsingContext();
            _observers = new Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>>();
            _references = new Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>>();
        }

        internal void Update(TableAnalyzeContext context)
        {
            SyncPropertyStates(context);
            CheckCycle(context);
        }

        internal ITableAnalysisSummary GetSummary()
        {
            return new TableAnalysisSummary(this);
        }

        private void SyncPropertyStates(TableAnalyzeContext context)
        {
            foreach (var removedDesc in context.TableModificationSummary.RemovedProperties)
            {
                RemovePropertyState(context, removedDesc);
            }

            foreach (var updatedDesc in context.TableModificationSummary.UpdatedProperties)
            {
                UpdatePropertyState(context, updatedDesc);
            }
        }

        private void UpdatePropertyState(TableAnalyzeContext context, PropertyDescriptor desc)
        {
            var formula =
                context.ScriptRepository.GetTableScript(desc.ID)!.GetPropertyFormula(desc.Name)!;

            if (context.PropertyExpressionRepository.GetPropertyExpression(desc) is { } oldExpr)
            {
                foreach (var oldReferenceExpr in oldExpr.GetInnerReferences())
                {
                    var oldReference = ExprToPropertyDescriptor(oldReferenceExpr, desc.ID, context);
                    RemoveReference(desc, oldReference);
                }
            }

            // TODO: 에러 처리 - 애초에 Parse 가 Result 를 반환해야 될 것 같은데
            var expr = _parser.Parse(_parsingContext, formula);
            context.PropertyExpressionRepository.SetPropertyExpression(desc, expr);
            foreach (var newReferenceExpr in expr.GetInnerReferences())
            {
                var newReference = ExprToPropertyDescriptor(newReferenceExpr, desc.ID, context);
                AddReference(desc, newReference);
            }
        }

        private void RemovePropertyState(TableAnalyzeContext context, PropertyDescriptor desc)
        {
            context.PropertyExpressionRepository.RemovePropertyExpression(desc);

            if (_references.TryGetValue(desc, out var references))
            {
                foreach (var reference in references)
                {
                    RemoveReference(desc, reference);
                }
            }

            if (_observers.TryGetValue(desc, out var observers))
            {
                foreach (var observer in observers)
                {
                    RemoveReference(observer, desc);
                }
            }
        }

        private PropertyDescriptor ExprToPropertyDescriptor(Expression expr, TableId scope, TableAnalyzeContext context)
        {
            var tableScriptRepository = context.ScriptRepository;
            if (expr.IsIdent)
            {
                var propertyName = expr.AsIdentifierName()!;
                if (tableScriptRepository.GetSelfPropertyOfReference(scope, propertyName) is
                    { } referenceDesc)
                    return referenceDesc;
            }
            else if (expr.IsPropertyExpr)
            {
                var (tableName, propertyName) = expr.AsProperty();
                if (tableScriptRepository.GetScopedPropertyOfReference(scope, tableName, propertyName) is
                    { } referenceDesc)
                    return referenceDesc;
            }

            throw new Exception("TODO: invalid reference");
        }

        private void AddReference(PropertyDescriptor desc, PropertyDescriptor referenceDesc)
        {
            if (!_references.ContainsKey(desc)) _references[desc] = new HashSet<PropertyDescriptor>();
            _references[desc].Add(referenceDesc);

            if (!_observers.ContainsKey(referenceDesc))
                _observers[referenceDesc] = new HashSet<PropertyDescriptor>();
            _observers[referenceDesc].Add(desc);
        }

        private void RemoveReference(PropertyDescriptor desc, PropertyDescriptor referenceDesc)
        {
            var references = _references[desc];
            references.Remove(referenceDesc);
            if (!references.Any()) _references.Remove(desc);

            var observers = _observers[referenceDesc];
            observers.Remove(desc);
            if (!observers.Any()) _observers.Remove(referenceDesc);
        }

        /// <summary>
        /// 참조 그래프 내의 모든 순환 경로를 찾아서, 거기에 포함된 모든 정점으로 구성된 집합을 Cycle 속성에 저장한다.
        /// </summary>
        private void CheckCycle(TableAnalyzeContext context)
        {
            _cycle = null;
            // TODO: 기억하고 있는 reference/observer 정보를 이용해 변경된 건에 한해서만 탐색하기 - 이미 cycle 이 있을때/없을때 다르게

            var notVisited = context.ScriptRepository.GetAllProperties().ToHashSet();
            var cycle = new HashSet<PropertyDescriptor>();

            while (notVisited.Any())
            {
                var visitedInPath = new HashSet<PropertyDescriptor>();
                var isCycle = Visit(notVisited.First(), visitedInPath);
                if (isCycle)
                {
                    cycle.UnionWith(visitedInPath);
                }
            }

            bool Visit(PropertyDescriptor desc, HashSet<PropertyDescriptor> visitedInPath)
            {
                if (visitedInPath.Contains(desc))
                {
                    return true;
                }

                notVisited.Remove(desc);
                visitedInPath.Add(desc);

                var result = false;
                if (_observers.TryGetValue(desc, out var observers))
                {
                    foreach (var observer in observers)
                    {
                        result |= Visit(observer, visitedInPath);
                    }
                }

                return result;
            }

            if (cycle.Any()) _cycle = cycle;
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

            bool ITableAnalysisSummary.IsCyclic => _analyzer._cycle is { };

            IEnumerable<PropertyDescriptor>? ITableAnalysisSummary.GetAllReferenceCycle()
            {
                return _analyzer._cycle;
            }

            bool ITableAnalysisSummary.IsPropertyInCycle(PropertyDescriptor desc)
            {
                return _analyzer._cycle?.Contains(desc) ?? false;
            }

            IEnumerable<PropertyDescriptor> ITableAnalysisSummary.GetPropertyInvalidationSet()
            {
                throw new NotImplementedException();
            }
        }
    }
}