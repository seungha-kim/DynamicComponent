using System;
using System.Collections.Generic;
using System.Linq;

namespace ObservableTable.Engine
{
    internal class TableAnalyzer
    {
        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _receivers;
        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _references;
        private readonly HashSet<PropertyDescriptor> _removedProperties;

        private readonly HashSet<PropertyDescriptor> _updatedProperties;
        private List<PropertyDescriptor>? Cycle { get; set; }
        public bool IsCyclic => Cycle is { };

        internal TableAnalysisResult Analyze(
            TableAnalyzeContext context)
        {
            foreach (var desc in _removedProperties)
            {
                // if (!_references.ContainsKey(desc)) return;

                var references = _references[desc]!;
                _references.Remove(desc);

                foreach (var reference in references)
                {
                    var receivers = _receivers[reference]!;
                    receivers.Remove(desc);
                    if (!receivers.Any()) _receivers.Remove(reference);
                }
            }

            UpdateDependencies(context.ScriptRepository, context.PropertyExpressionRepository);

            throw new NotImplementedException();
        }

        private void UpdateDependencies(TableScriptRepository tableScriptRepository,
            PropertyExpressionRepository propertyExpressionRepository)
        {
            foreach (var desc in _updatedProperties)
            {
                var expr = propertyExpressionRepository.GetPropertyExpression(desc!);
                foreach (var reference in expr.GetInnerReferences())
                {
                    if (reference.IsIdent)
                    {
                        var propertyName = reference.AsIdentifierName()!;
                        if (tableScriptRepository.GetSelfPropertyOfReference(desc.ID, propertyName) is
                            { } referenceDesc)
                            AddDependency(desc, referenceDesc);
                    }

                    if (reference.IsPropertyExpr)
                    {
                        var (tableName, propertyName) = reference.AsProperty();
                        if (tableScriptRepository.GetScopedPropertyOfReference(desc.ID, tableName, propertyName) is
                            { } referenceDesc)
                            AddDependency(desc, referenceDesc);
                    }
                }
            }
        }

        private void AddDependency(PropertyDescriptor desc, PropertyDescriptor referenceDesc)
        {
            if (!_references.ContainsKey(desc)) _references[desc] = new HashSet<PropertyDescriptor>();
            _references[desc].Add(referenceDesc);

            if (!_receivers.ContainsKey(referenceDesc))
                _receivers[referenceDesc] = new HashSet<PropertyDescriptor>();
            _receivers[referenceDesc].Add(desc);
        }

        internal IEnumerable<PropertyDescriptor> GetSenders(PropertyDescriptor desc)
        {
            if (!(_references[desc] is { } references)) yield break;
            foreach (var r in references) yield return r;
        }

        internal IEnumerable<PropertyDescriptor> GetReceivers(PropertyDescriptor desc)
        {
            if (!(_receivers[desc] is { } receivers)) yield break;
            foreach (var r in receivers) yield return r;
        }

        internal IEnumerable<PropertyDescriptor>? GetPropertyCycle()
        {
            return Cycle;
        }

        internal bool IsPropertyInCycle(PropertyDescriptor desc)
        {
            throw new NotImplementedException();
        }

        internal void RemoveTable(TableId id)
        {
            throw new NotImplementedException();
        }

        internal void HandlePropertyRemoved(PropertyDescriptor desc)
        {
            _removedProperties.Add(desc);
        }

        internal void HandlePropertyUpdated(PropertyDescriptor desc)
        {
            _updatedProperties.Add(desc);
        }

        internal bool CheckDependencyCycle()
        {
            throw new NotImplementedException();
        }
    }
}