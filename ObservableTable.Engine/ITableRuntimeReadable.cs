using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal interface ITableRuntimeReadable
    {
        TableRuntime? GetTableById(TableId id);
        TableRuntime? GetParent(TableId id);
        IEnumerable<TableRuntime> GetChildrenById(TableId id);
    }
}