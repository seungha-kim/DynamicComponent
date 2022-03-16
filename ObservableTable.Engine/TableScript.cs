using System;
using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableScript
    {
        public delegate void ParentUpdateDelegate(TableScript sender, TableId? oldParent, TableId? newParent);

        public delegate void PropertyInvalidateDelegate(TableScript sender, string propertyName);

        private TableId? _parentId;

        internal TableScript(TableId id, string name)
        {
            ID = id;
            Name = name;
        }

        public TableId ID { get; }
        public string Name { get; }

        public TableId? ParentId
        {
            get => _parentId;
            set
            {
                if (value == ID) throw new Exception("TODO: Cannot be parent of self");
                var oldParentId = _parentId;
                _parentId = value;
                OnParentUpdate.Invoke(this, oldParentId, _parentId);
            }
        }

        private Dictionary<string, string> Formulas { get; } = new Dictionary<string, string>();

        public event PropertyInvalidateDelegate OnPropertyFormulaUpdate = delegate { };
        public event PropertyInvalidateDelegate OnPropertyFormulaRemoved = delegate { };

        // TODO: 이 때 dependency 완전 다시 계산해야겠는데..
        public event ParentUpdateDelegate OnParentUpdate = delegate { };

        public IEnumerable<string> GetPropertyNames()
        {
            return Formulas.Keys;
        }

        public bool HasProperty(string name)
        {
            return Formulas.ContainsKey(name);
        }

        public string? GetPropertyFormula(string name)
        {
            return Formulas.TryGetValue(name, out var value) ? value : null;
        }

        public void UpdatePropertyFormula(string name, string formula)
        {
            Formulas[name] = formula;
            OnPropertyFormulaUpdate.Invoke(this, name);
        }

        public void RemovePropertyFormula(string name)
        {
            Formulas.Remove(name);
            OnPropertyFormulaRemoved.Invoke(this, name);
        }
    }
}