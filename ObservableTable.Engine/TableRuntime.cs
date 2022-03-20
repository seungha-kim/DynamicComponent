using System.Collections.Generic;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    public class TableRuntime
    {
        public delegate void PropertyUpdateDelegate(TableRuntime sender, string propertyName, FormulaValue value);

        private TableScript _sourceScript;
        private Dictionary<string, FormulaValue> _values;

        public TableId ID => _sourceScript.ID;

        // public bool HasCopy { get; }
        public TableId? ParentId => _sourceScript.ParentId;
        public string Name => _sourceScript.Name;

        public TableRuntime(TableScript sourceScript)
        {
            _sourceScript = sourceScript;
            _values = new Dictionary<string, FormulaValue>();
        }

        public event PropertyUpdateDelegate OnPropertyUpdate = delegate { };

        public FormulaValue? GetProperty(string name)
        {
            return _values.TryGetValue(name, out var result) ? result : null;
        }

        public FormulaValue? GetPropertyOfCopy(string name, int index)
        {
            return _values.TryGetValue(name, out var result) ? result : null; // TODO
        }

        internal void UpdateProperty(string name, FormulaValue value)
        {
            _values[name] = value;
        }
    }
}