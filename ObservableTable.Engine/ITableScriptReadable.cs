using System;
using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal interface ITableScriptReadable
    {
        TableScript GetTableScript(TableId id);
        IEnumerable<TableScript> GetChildren(TableId id);
    }
}
