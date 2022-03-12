using System.Collections.Generic;
using Formula.AST;
using Formula.Interface;

namespace ObservableTable.Engine
{
    public class TableScript
    {
        public delegate void ScriptUpdateDelegate(TableScript sender, string propertyName);

        public delegate void ParentUpdateDelegate(TableScript sender);

        public event ScriptUpdateDelegate OnPropertyFormulaUpdate = delegate { };
        public event ParentUpdateDelegate OnParentUpdate = delegate { };
        public TableId ID { get; }
        public string Name { get; }

        public TableScript? Parent
        {
            get => _parent;
            set
            {
                _parent = value;
                OnParentUpdate.Invoke(this);
            }
        }

        internal Dictionary<string, Expression> ExpressionCache { get; } =
            new Dictionary<string, Expression>();

        private TableScript? _parent;
        private Dictionary<string, string> Formulas { get; } = new Dictionary<string, string>();
        private IFormulaParser Parser { get; }

        public TableScript(TableId id, string name, IFormulaParser parser)
        {
            ID = id;
            Name = name;
            Parser = parser;
        }

        public IEnumerable<string> GetPropertyNames()
        {
            return Formulas.Keys;
        }

        public string? GetPropertyFormula(string name)
        {
            return Formulas.TryGetValue(name, out var value) ? value : null;
        }

        public void UpdatePropertyFormula(string name, string formula)
        {
            Formulas[name] = formula;
            ExpressionCache.Remove(name);
            OnPropertyFormulaUpdate.Invoke(this, name);
        }

        public void RemovePropertyFormula(string name)
        {
            Formulas.Remove(name);
            ExpressionCache.Remove(name);
        }
    }
}