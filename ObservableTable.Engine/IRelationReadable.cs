using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal interface IRelationReadable
    {
        IEnumerable<PropertyDescriptor> GetSenders(PropertyDescriptor desc);

        IEnumerable<PropertyDescriptor> GetReceivers(PropertyDescriptor desc);
    }
}