using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableExecuteContext
    {
        internal PropertyExpressionRepository PropertyExpressionRepository { get; set; }
        internal TableRuntimeRepository RuntimeRepository { get; set; }
        internal ITableAnalysisSummary AnalysisSummary { get; set; }
        internal TableModificationSummary TableModificationSummary { get; set; }
    }
}