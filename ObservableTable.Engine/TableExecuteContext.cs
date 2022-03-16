using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableExecuteContext
    {
        internal PropertyExpressionRepository PropertyExpressionRepository { get; set; }
        internal TableRuntimeRepository RuntimeRepository { get; set; }

        internal TableAnalysisResult AnalysisResult { get; set; }

        internal IEnumerable<TableId> CreatedTableIds { get; set; }
        internal IEnumerable<TableId> RemovedTableIds { get; set; }
        internal IEnumerable<(TableId table, TableId? oldParent, TableId? newParent)> ParentUpdates { get; set; }
        internal IEnumerable<PropertyDescriptor> UpdatedProperties { get; set; }
        internal IEnumerable<PropertyDescriptor> RemovedProperties { get; set; }
    }
}