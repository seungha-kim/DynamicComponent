using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.Interface;

namespace ObservableTable.Engine
{
    public class TableScriptRepository : ITableScriptReadable
    {
        public delegate void PropertyInvalidateDelegate(PropertyDescriptor desc);

        public delegate void TableInvalidateDelegate(TableId id);

        public delegate void TableParentUpdateDelegate(TableId id, TableId? oldParent, TableId? newParent);

        // Data
        private readonly Dictionary<TableId, TableScript> _scripts = new Dictionary<TableId, TableScript>();

        public TableScript GetTableScript(TableId id)
        {
            if (_scripts.TryGetValue(id, out var script))
                return script;
            throw new Exception("TODO: Non-existing ID");
        }

        public IEnumerable<TableScript> GetChildren(TableId id)
        {
            var script = _scripts[id];
            if (script is null) throw new Exception("TODO: Non-existent id");

            foreach (var pair in _scripts.Where(pair => pair.Value.ParentId == id)) yield return pair.Value;
        }

        public TableScript? GetParent(TableId id)
        {
            if (!(_scripts[id]?.ParentId is { } parentId)) return null;
            return _scripts[parentId];
        }

        public event TableParentUpdateDelegate OnParentUpdate = delegate { };

        public event PropertyInvalidateDelegate OnPropertyUpdated = delegate { };
        public event PropertyInvalidateDelegate OnPropertyRemoved = delegate { };
        public event TableInvalidateDelegate OnTableCreated = delegate { };
        public event TableInvalidateDelegate OnTableRemoved = delegate { };

        public TableScript CreateTableScript(TableId id, string name)
        {
            if (_scripts.ContainsKey(id))
                // TODO: custom error
                throw new Exception("Already has script with same ID");

            var script = new TableScript(id, name);

            _scripts[id] = script;
            script.OnPropertyFormulaUpdate += HandlePropertyFormulaUpdate;
            script.OnPropertyFormulaRemoved += HandlePropertyFormulaRemoved;
            script.OnParentUpdate += HandleParentUpdate;
            OnTableCreated.Invoke(id);

            return script;
        }

        internal PropertyDescriptor? GetSelfPropertyOfReference(TableId id, string identifier)
        {
            if (_scripts[id] is { } script && script.HasProperty(identifier))
                return new PropertyDescriptor(id, identifier);
            return null;
        }

        internal PropertyDescriptor? GetScopedPropertyOfReference(TableId id, string identifier, string propertyName)
        {
            if (!(GetParent(id) is { } parent)) return null;
            if (parent.Name == identifier && parent.HasProperty(propertyName))
                return new PropertyDescriptor(parent.ID, propertyName);
            foreach (var child in GetChildren(parent.ID))
                if (child.Name == identifier && child.HasProperty(propertyName))
                    return new PropertyDescriptor(child.ID, propertyName);
            return null;
        }

        public void RemoveTableScript(TableId id)
        {
            if (!_scripts.ContainsKey(id)) return;

            var script = _scripts[id];
            script.OnPropertyFormulaUpdate -= HandlePropertyFormulaUpdate;
            script.OnPropertyFormulaRemoved -= HandlePropertyFormulaRemoved;

            _scripts.Remove(id);
            OnTableRemoved.Invoke(id);
        }

        private void HandlePropertyFormulaUpdate(TableScript sender, string propertyName)
        {
            var desc = new PropertyDescriptor(sender.ID, propertyName);
            OnPropertyUpdated.Invoke(desc);
        }

        private void HandlePropertyFormulaRemoved(TableScript sender, string propertyName)
        {
            var desc = new PropertyDescriptor(sender.ID, propertyName);
            OnPropertyRemoved.Invoke(desc);
        }

        private void HandleParentUpdate(TableScript sender, TableId? oldParent, TableId? newParent)
        {
            OnParentUpdate.Invoke(sender.ID, oldParent, newParent);
        }
    }
}