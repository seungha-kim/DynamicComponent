using Formula.ValueRepresentation;

namespace ObservableTable.Engine
{
    public class Table
    {
        public delegate void PropertyUpdateDelegate(Table sender, string propertyName, FormulaValue value);
        public event PropertyUpdateDelegate OnPropertyUpdate = delegate {  };
        public TableId ID { get; }
        public bool HasCopy { get; }
        public FormulaValue GetProperty(string name)
        {
            throw new System.NotImplementedException();
        }

        public FormulaValue GetPropertyOfCopy(string name, int index)
        {
            throw new System.NotImplementedException();
        }

        internal void UpdateProperty(string name, FormulaValue value)
        {
            throw new System.NotImplementedException();
        }
    }
}