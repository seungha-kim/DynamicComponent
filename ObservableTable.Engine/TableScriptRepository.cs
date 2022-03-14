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

        // Components
        private readonly IFormulaParser _parser = Parsing.createParser();
        private readonly ParsingContext _parsingContext = new ParsingContext();

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

            foreach (var pair in _scripts.Where(pair => pair.Value.Parent?.ID == id)) yield return pair.Value;
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
            var formula = sender.GetPropertyFormula(propertyName)!;
            var expr = _parser.Parse(_parsingContext, formula);
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

        private class ParsingContext : IParsingContext
        {
        }
    }
}