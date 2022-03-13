using System;
using System.Collections.Generic;
using System.Linq;
using Formula;
using Formula.AST;
using Formula.Interface;

namespace ObservableTable.Engine
{
    // TableScript의 포뮬러 파싱과 정적구조 분석을 담당
    public class TableScriptManager : ITableScriptReadable
    {
        public delegate void PropertyInvalidateDelegate(PropertyDescriptor desc);

        public delegate void TableInvalidateDelegate(TableId id);

        // Components
        private readonly RelationManager _relationManager;
        private readonly IFormulaParser _parser = Parsing.createParser();
        private readonly ParsingContext _parsingContext = new ParsingContext();

        private readonly Dictionary<PropertyDescriptor, Expression> _propertyExpressions =
            new Dictionary<PropertyDescriptor, Expression>();

        // Data
        private readonly Dictionary<TableId, TableScript> _scripts = new Dictionary<TableId, TableScript>();

        internal TableScriptManager(RelationManager relationManager)
        {
            _relationManager = relationManager;
        }

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

        public event PropertyInvalidateDelegate OnPropertyInvalidated = delegate { };
        public event TableInvalidateDelegate OnTableInvalidated = delegate { };

        public TableScript CreateTableScript(TableId id, string name)
        {
            if (_scripts.ContainsKey(id))
                // TODO: custom error
                throw new Exception("Already has script with same ID");

            var script = new TableScript(id, name);

            _scripts[id] = script;
            script.OnPropertyFormulaUpdate += HandlePropertyFormulaUpdate;
            OnTableInvalidated.Invoke(id);

            return script;
        }

        public void RemoveTableScript(TableId id)
        {
            if (!_scripts.ContainsKey(id)) return;

            var script = _scripts[id];
            script.OnPropertyFormulaUpdate -= HandlePropertyFormulaUpdate;
            _scripts.Remove(id);
            foreach (var propertyName in script.GetPropertyNames())
            {
                var desc = new PropertyDescriptor(id, propertyName);
                _propertyExpressions.Remove(desc);
            }

            OnTableInvalidated.Invoke(id);
        }

        private void HandlePropertyFormulaUpdate(TableScript sender, string propertyName)
        {
            var desc = new PropertyDescriptor(sender.ID, propertyName);
            if (sender.HasProperty(propertyName))
            {
                var formula = sender.GetPropertyFormula(propertyName)!;
                var expr = _parser.Parse(_parsingContext, formula);
                _propertyExpressions[desc] = expr;
                _relationManager.UpdateReferences(desc, expr);
            }
            else
            {
                _propertyExpressions.Remove(desc);
                _relationManager.RemoveReferences(desc);
            }

            OnPropertyInvalidated.Invoke(desc);
        }

        private class ParsingContext : IParsingContext
        {
        }
    }
}