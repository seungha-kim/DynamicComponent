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
            if (!_tables.TryGetValue(id, out var table)) return null;
            if (!(table.ParentId is { } parentId)) return null;
            return _tables[parentId];
        }

        public TableRuntime CreateTable(TableScript sourceScript)
        {
            var id = sourceScript.ID;
            if (_tables.ContainsKey(id)) throw new Exception("TODO: existing id");
            var result = new TableRuntime(sourceScript);
            _tables[id] = result;
            return result;
        }
    }
}