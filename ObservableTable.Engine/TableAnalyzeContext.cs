using System.Collections.Generic;

namespace ObservableTable.Engine
{
    internal class TableAnalyzeContext
    {
        internal TableScriptRepository ScriptRepository { get; set; }
        internal PropertyExpressionRepository PropertyExpressionRepository { get; set; }
        internal TableModificationSummary TableModificationSummary { get; set; }
    }
}