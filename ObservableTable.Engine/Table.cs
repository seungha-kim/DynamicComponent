namespace ObservableTable.Engine
{
    public class Table
    {
        public delegate void PropertyUpdateDelegate(Table sender, string propertyName);
        public event PropertyUpdateDelegate OnPropertyUpdate = delegate {  };
        public TableId ID { get; }
        public bool HasCopy { get; }
        public float GetNumberProperty(string name)
        {
            throw new System.NotImplementedException();
        }

        public string GetTextProperty(string name)
        {
            throw new System.NotImplementedException();
        }

        public float GetNumberPropertyOfCopy(string name, int index)
        {
            throw new System.NotImplementedException();
        }

        internal void UpdateNumberProperty(string name, float value)
        {
            throw new System.NotImplementedException();
        }

        internal void UpdateTextProperty(string name, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}