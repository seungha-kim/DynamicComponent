using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    public class Table
    {
        public delegate void PropertyUpdateDelegate(Table sender, string propertyName, FormulaValue value);
        public event PropertyUpdateDelegate OnPropertyUpdate = delegate {  };
        public TableId ID => _sourceScript.ID;
        public bool HasCopy { get; }
        public TableId? ParentId => _sourceScript.Parent?.ID;
        public string Name => _sourceScript.Name;

        private TableScript _sourceScript;
        
        public FormulaValue? GetProperty(string name)
        {
            throw new System.NotImplementedException();
        }

        public FormulaValue? GetPropertyOfCopy(string name, int index)
        {
            throw new System.NotImplementedException();
        }

        internal void UpdateProperty(string name, FormulaValue value)
        {
            throw new System.NotImplementedException();
        }
    }
}