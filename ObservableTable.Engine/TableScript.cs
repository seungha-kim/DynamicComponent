using System;
using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableScript
    {
        public delegate void ParentUpdateDelegate(TableScript sender);

        public delegate void ScriptUpdateDelegate(TableScript sender, string propertyName);

        private TableScript? _parent;

        internal TableScript(TableId id, string name)
        {
            ID = id;
            Name = name;
        }

        public TableId ID { get; }
        public string Name { get; }

        public TableScript? Parent
        {
            get => _parent;
            set
            {
                if (value == this) throw new Exception("TODO: Cannot be parent of self");
                _parent = value;
                OnParentUpdate.Invoke(this);
            }
        }

        private Dictionary<string, string> Formulas { get; } = new Dictionary<string, string>();

        public event ScriptUpdateDelegate OnPropertyFormulaUpdate = delegate { };

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
            OnPropertyFormulaUpdate.Invoke(this, name);
        }
    }
}