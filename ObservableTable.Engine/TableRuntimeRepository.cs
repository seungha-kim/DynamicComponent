using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.AST;
using Formula.Interface;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    internal class TableRuntimeRepository : ITableRuntimeReadable
    {
        private readonly ITableScriptReadable _scriptReadable;
        private readonly Dictionary<TableId, TableRuntime> _tables;

        internal TableRuntimeRepository()
        {
            _tables = new Dictionary<TableId, TableRuntime>();
        }

        public TableRuntime? GetTableById(TableId id)
        {
            return _tables.TryGetValue(id, out var table) ? table : null;
        }

        public TableRuntime? GetParent(TableId id)
        {
            if (!(_scriptReadable.GetTableScript(id)?.ParentId is { } parentId)) return null;
            return _tables[parentId];
        }

        public IEnumerable<TableRuntime> GetChildrenById(TableId id)
        {
            return _scriptReadable.GetChildren(id).Select(script => _tables[script.ID]!);
        }
    }
}