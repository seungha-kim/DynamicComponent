﻿using System;
using System.Collections.Generic;
using System.Linq;
using Formula.AST;
using Formula.Interface;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    internal class TableRuntimeManager: ITableRuntimeReadable
    {
        private readonly Dictionary<TableId, TableRuntime> _tables;
        private readonly Stack<PropertyDescriptor> _propertyDfs;
        private readonly IRelationReadable _relationReadable;
        private readonly ITableScriptReadable _scriptReadable;
        private readonly IFormulaEvaluator _evaluator;
        
        private readonly HashSet<PropertyDescriptor> _invalidatingProperties;
        private readonly HashSet<TableId> _invalidatingTables;

        internal TableRuntimeManager(ITableScriptReadable scriptManager, IRelationReadable relationReadable)
        {
            _relationReadable = relationReadable;
            _tables = new Dictionary<TableId, TableRuntime>();
            _propertyDfs = new Stack<PropertyDescriptor>();
            _evaluator = Formula.Evaluation.createEvaluator();
            _invalidatingProperties = new HashSet<PropertyDescriptor>();
            _invalidatingTables = new HashSet<TableId>();
        }

        internal void InvalidateTable(TableId id)
        {
            if (!_tables.ContainsKey(id))
            {
                _tables[id] = new TableRuntime();
            }
            else if (_tables.ContainsKey(id) && _scriptReadable.GetTableScript(id) is null)
            {
                _tables.Remove(id);
            }
        }

        internal void InvalidateProperty(PropertyDescriptor desc)
        {
            _invalidatingProperties.Add(desc);
        }

        internal void Run()
        {
            // TODO: 순환참조 알림
            // TODO: parent update 때 일단 전체 의존성 재계산
            if (_relationReadable.IsCyclic) return;
            InvalidatePropertiesByAnimation();
            UpdateProperties();
            PostRun();
        }

        private void InvalidatePropertiesByAnimation()
        {
            // TODO
        }

        private void PostRun()
        {
            _invalidatingTables.Clear();
            _invalidatingProperties.Clear();
        }

        public TableRuntime? GetTableById(TableId id)
        {
            return _tables.TryGetValue(id, out var table) ? table : null;
        }

        public TableRuntime? GetParent(TableId id)
        {
            var script = _scriptReadable.GetTableScript(id);
            if (script is null) throw new Exception("TODO: Non-existent id");
            var parentScript = script?.Parent;
            if (parentScript is null) return null;

            _tables.TryGetValue(parentScript.ID, out var parent);
            return parent;
        }

        public IEnumerable<TableRuntime> GetChildrenById(TableId id)
        {
            return _scriptReadable.GetChildren(id).Select(script => _tables[script.ID]!);
        }

        private void UpdateProperties()
        {
            while (_invalidatingProperties.Any())
            {
                // TODO: 재귀?
                // _executor.UpdateProperty(script, table, desc.Name);
            }
        }

        
        private void UpdateProperty(TableRuntime tableRuntime, string propertyName, Expression expr)
        {
            var ctx = new EvaluationContext(tableRuntime.ID, this);
            var result = _evaluator.Evaluate(ctx, expr);
            tableRuntime.UpdateProperty(propertyName, result);
        }

        private class EvaluationContext : IEvaluationContext
        {
            internal EvaluationContext(TableId id, ITableRuntimeReadable tableRuntimeReadable)
            {
                ID = id;
                TableRuntimeReadable = tableRuntimeReadable;
            }

            private TableId ID { get; }
            private ITableRuntimeReadable TableRuntimeReadable { get; }

            public FormulaValue GetIdentifierValue(string name)
            {
                var table = TableRuntimeReadable.GetTableById(ID);
                if (table is null) return FormulaValue.NullValue;

                var prop = table.GetProperty(name);
                return prop ?? FormulaValue.NullValue;
            }

            public FormulaValue GetPropertyValue(string receiver, string name)
            {
                // TODO: self property?

                var parent = TableRuntimeReadable.GetParent(ID);
                if (parent is null) return FormulaValue.NullValue;
                if (parent.Name == receiver)
                {
                    var parentProp = parent.GetProperty(name);
                    if (parentProp is { }) return parentProp;
                }

                foreach (var sibling in TableRuntimeReadable.GetChildrenById(parent.ID))
                {
                    if (sibling.Name != receiver) continue;

                    var siblingProp = sibling.GetProperty(name);
                    if (siblingProp is { }) return siblingProp;
                }

                return FormulaValue.NullValue;
            }
        }
    }
}