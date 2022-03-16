using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal class TableAnalyzeContext
    {
        internal TableScriptRepository ScriptRepository { get; set; }
        internal PropertyExpressionRepository PropertyExpressionRepository { get; set; }

        internal IEnumerable<TableId> CreatedTableIds { get; set; }
        internal IEnumerable<TableId> RemovedTableIds { get; set; }
        internal IEnumerable<(TableId table, TableId? oldParent, TableId? newParent)> ParentUpdates { get; set; }
        internal IEnumerable<PropertyDescriptor> UpdatedProperties { get; set; }
        internal IEnumerable<PropertyDescriptor> RemovedProperties { get; set; }
    }
}