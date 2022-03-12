using System.Collections.Generic;
using System.Linq;

namespace ObservableTable.Engine
{
    public class TableEngine
    {
        private Dictionary<TableId, TableScript> Scripts { get; } = new Dictionary<TableId, TableScript>();
        private Dictionary<TableId, Table> Tables { get; } = new Dictionary<TableId, Table>();
        private Queue<TableId> TableCreationQueue { get; } = new Queue<TableId>();
        private DependencyManager DepsManager { get; } = new DependencyManager();
        private bool NeedsInvalidateDependencies => ScriptPropertyUpdateQueue.Any();
        private Queue<PropertyDescriptor> ScriptPropertyUpdateQueue { get; } = new Queue<PropertyDescriptor>();
        private Queue<PropertyDescriptor> TablePropertyUpdateQueue { get; } = new Queue<PropertyDescriptor>();
        private Stack<PropertyDescriptor> PropertyDfs { get; } = new Stack<PropertyDescriptor>();
        private FormulaExecutor Executor { get; } = new FormulaExecutor();

        public void AddTableScript(TableScript script)
        {
            Scripts[script.ID] = script;
            script.OnPropertyFormulaUpdate += HandlePropertyFormulaUpdate;
            TableCreationQueue.Enqueue(script.ID);
        }

        public TableScript? GetTableScript(TableId id)
        {
            return Scripts.TryGetValue(id, out var script) ? script : null;
        }

        public void RemoveTableScript(TableId id)
        {
            var script = Scripts[id];
            script.OnPropertyFormulaUpdate -= HandlePropertyFormulaUpdate;
            Scripts.Remove(id);
        }

        public void Run()
        {
            if (NeedsInvalidateDependencies)
            {
                DepsManager.AnalysisDependencies(Scripts);
            }

            if (DepsManager.IsCyclic) return;

            SyncTables();
            UpdatePropertiesByScriptUpdate();
            UpdatePropertiesByAnimation();
            UpdateDependentProperties();
        }

        public Table GetTableById(TableId id)
        {
            return Tables.TryGetValue(id, out var table) ? table : null;
        }

        private void UpdateProperty(in PropertyDescriptor desc)
        {
            var script = Scripts[desc.ID];
            var table = Tables[desc.ID];
            Executor.UpdateProperty(script, table, desc.Name);
        }

        private void SyncTables()
        {
            while (TableCreationQueue.Any())
            {
                var id = TableCreationQueue.Dequeue();
                var script = Scripts[id];
                var table = new Table();
                Tables[id] = table;
                foreach (var name in script.GetPropertyNames())
                {
                    ScriptPropertyUpdateQueue.Enqueue(new PropertyDescriptor(script.ID, name));
                }
            }
        }

        private void UpdatePropertiesByScriptUpdate()
        {
            while (ScriptPropertyUpdateQueue.Any())
            {
                var desc = ScriptPropertyUpdateQueue.Dequeue();
                UpdateProperty(desc);
                TablePropertyUpdateQueue.Enqueue(desc);
            }
        }

        private void UpdatePropertiesByAnimation()
        {
            // TODO
        }

        private void UpdateDependentProperties()
        {
            while (TablePropertyUpdateQueue.Any())
            {
                var firstProp = TablePropertyUpdateQueue.Dequeue();
                PropertyDfs.Clear();
                PropertyDfs.Push(firstProp);

                while (PropertyDfs.Any())
                {
                    var desc = PropertyDfs.Pop();
                    UpdateProperty(desc);
                    foreach (var receiver in DepsManager.GetReceivers(desc))
                    {
                        PropertyDfs.Push(receiver);
                    }
                }
            }
        }

        private void HandlePropertyFormulaUpdate(TableScript sender, string propertyName)
        {
            ScriptPropertyUpdateQueue.Enqueue(new PropertyDescriptor(sender.ID, propertyName));
        }
    }
}