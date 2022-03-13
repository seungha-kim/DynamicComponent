using System;
using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    public class TableRuntime
    {
        public delegate void PropertyUpdateDelegate(TableRuntime sender, string propertyName, FormulaValue value);

        private TableScript _sourceScript;
        public TableId ID => _sourceScript.ID;
        public bool HasCopy { get; }
        public TableId? ParentId => _sourceScript.Parent?.ID;
        public string Name => _sourceScript.Name;

        public event PropertyUpdateDelegate OnPropertyUpdate = delegate { };

        public FormulaValue? GetProperty(string name)
        {
            throw new NotImplementedException();
        }

        public FormulaValue? GetPropertyOfCopy(string name, int index)
        {
            throw new NotImplementedException();
        }

        internal void UpdateProperty(string name, FormulaValue value)
        {
            throw new NotImplementedException();
        }
    }
}