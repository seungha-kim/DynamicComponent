using System;
using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal interface ITableAnalysisSummary
    {
        internal IEnumerable<PropertyDescriptor> GetReferences(PropertyDescriptor desc);
        internal IEnumerable<PropertyDescriptor> GetObservers(PropertyDescriptor desc);
        internal bool IsCyclic { get; }
        internal IEnumerable<PropertyDescriptor>? GetReferenceCycle();
        internal bool IsPropertyInCycle(PropertyDescriptor desc);
    }
}