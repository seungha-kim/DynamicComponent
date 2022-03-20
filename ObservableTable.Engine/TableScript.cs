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

        private readonly Dictionary<string, string> _formulas = new Dictionary<string, string>();

        public event PropertyInvalidateDelegate OnPropertyFormulaUpdate = delegate { };
        public event PropertyInvalidateDelegate OnPropertyFormulaRemoved = delegate { };

        // TODO: 이 때 dependency 완전 다시 계산해야겠는데..
        public event ParentUpdateDelegate OnParentUpdate = delegate { };

        public IEnumerable<string> GetPropertyNames()
        {
            return _formulas.Keys;
        }

        public bool HasProperty(string name)
        {
            return _formulas.ContainsKey(name);
        }

        public string? GetPropertyFormula(string name)
        {
            return _formulas.TryGetValue(name, out var value) ? value : null;
        }

        public void UpdatePropertyFormula(string name, string formula)
        {
            _formulas[name] = formula;
            OnPropertyFormulaUpdate.Invoke(this, name);
        }

        public void RemovePropertyFormula(string name)
        {
            _formulas.Remove(name);
            OnPropertyFormulaRemoved.Invoke(this, name);
        }
    }
}