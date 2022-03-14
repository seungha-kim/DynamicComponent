using System;
using System.Collections.Generic;
using System.Linq;
using Formula.AST;

namespace ObservableTable.Engine
{
    internal class TableAnalyzer : IRelationReadable
    {
        private readonly Dictionary<PropertyDescriptor, Expression> _propertyExpressions =
            new Dictionary<PropertyDescriptor, Expression>();

        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _receivers;
        private readonly Dictionary<PropertyDescriptor, HashSet<PropertyDescriptor>> _references;
        private List<PropertyDescriptor>? Cycle { get; set; }
        public bool IsCyclic => Cycle is { };

        public IEnumerable<PropertyDescriptor> GetSenders(PropertyDescriptor desc)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PropertyDescriptor> GetReceivers(PropertyDescriptor desc)
        {
            throw new NotImplementedException();
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

        internal void RemoveProperty(PropertyDescriptor desc)
        {
            if (!_references.ContainsKey(desc)) return;

            var references = _references[desc]!;
            _references.Remove(desc);

            foreach (var reference in references)
            {
                var receivers = _receivers[reference]!;
                receivers.Remove(desc);
                if (!receivers.Any()) _receivers.Remove(reference);
            }
        }

        internal void UpdateProperty(PropertyDescriptor desc)
        {
            var expr = _propertyExpressions[desc]!;
            foreach (var reference in expr.GetInnerReferences())
            {
                var isInScope = false;
                isInScope |= reference.IsIdent && CanReference(reference.IdentifierName);
                isInScope |= reference.IsPropertyExpr && CanReference(Expression.PropertyExpr);
                if (isInScope)
                {
                    var refDesc = reference.AsPropertyDescriptor;

                    if (!_references.ContainsKey(desc)) _references[desc] = new HashSet<PropertyDescriptor>();
                    _references[desc].Add(refDesc);

                    if (!_receivers.ContainsKey(refDesc))
                        _receivers[refDesc] = new HashSet<PropertyDescriptor>();
                    _receivers[refDesc].Add(desc);
                }
            }
        }

        internal bool CheckDependencyCycle()
        {
            var visited = new HashSet<PropertyDescriptor>();

            return false;
        }
    }
}