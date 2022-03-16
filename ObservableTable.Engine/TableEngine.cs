using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableEngine
    {
        private readonly TableScriptRepository _scriptRepository;
        private readonly PropertyExpressionRepository _propertyExpressionRepository;
        private readonly TableRuntimeRepository _runtimeRepository;

        private readonly TableAnalyzer _tableAnalyzer;
        private readonly TableExecutor _tableExecutor;

        private readonly HashSet<TableId> _createdTableIds;
        private readonly HashSet<TableId> _removedTableIds;
        private readonly HashSet<(TableId table, TableId? oldParent, TableId? newParent)> _parentUpdates;
        private readonly HashSet<PropertyDescriptor> _updatedProperties;
        private readonly HashSet<PropertyDescriptor> _removedProperties;

        public TableEngine()
        {
            _scriptRepository = new TableScriptRepository();
            _propertyExpressionRepository = new PropertyExpressionRepository();
            _tableAnalyzer = new TableAnalyzer();
            _runtimeRepository = new TableRuntimeRepository();
            _tableExecutor = new TableExecutor();

            _createdTableIds = new HashSet<TableId>();
            _removedTableIds = new HashSet<TableId>();
            _parentUpdates = new HashSet<(TableId table, TableId? oldParent, TableId? newParent)>();
            _updatedProperties = new HashSet<PropertyDescriptor>();
            _removedProperties = new HashSet<PropertyDescriptor>();

            _scriptRepository.OnTableCreated += id => _createdTableIds.Add(id);
            _scriptRepository.OnTableRemoved += id => _removedTableIds.Add(id);
            _scriptRepository.OnParentUpdate += (id, parent, newParent) => _parentUpdates.Add((id, parent, newParent));
            _scriptRepository.OnPropertyUpdated += desc => _updatedProperties.Add(desc);
            _scriptRepository.OnPropertyRemoved += desc => _removedProperties.Add(desc);
        }

        public void Update()
        {
            var analysisResult = _tableAnalyzer.Analyze(new TableAnalyzeContext()
            {
                ScriptRepository = _scriptRepository,
                PropertyExpressionRepository = _propertyExpressionRepository,
                CreatedTableIds = _createdTableIds,
                RemovedTableIds = _removedTableIds,
                ParentUpdates = _parentUpdates,
                UpdatedProperties = _updatedProperties,
                RemovedProperties = _removedProperties,
            });

            _tableExecutor.Execute(new TableExecuteContext()
            {
                PropertyExpressionRepository = _propertyExpressionRepository,
                RuntimeRepository = _runtimeRepository,
                AnalysisResult = analysisResult,
                CreatedTableIds = _createdTableIds,
                RemovedTableIds = _removedTableIds,
                ParentUpdates = _parentUpdates,
                UpdatedProperties = _updatedProperties,
                RemovedProperties = _removedProperties,
            });

            _createdTableIds.Clear();
            _removedTableIds.Clear();
            _parentUpdates.Clear();
            _updatedProperties.Clear();
            _removedProperties.Clear();
        }
    }
}