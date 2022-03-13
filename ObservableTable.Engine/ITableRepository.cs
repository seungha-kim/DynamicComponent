using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal interface ITableRepository
    {
        Table? GetTableById(TableId id);
        Table? GetParent(TableId id);
        IEnumerable<Table> GetChildrenById(TableId id);
    }
}