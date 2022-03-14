namespace ObservableTable.Engine
{
    public class TableEngine
    {
        private readonly TableRuntimeRepository _runtimeRepository;
        private readonly TableScriptRepository _scriptRepository;
        private readonly TableAnalyzer _tableAnalyzer;
        private readonly TableRunner _tableRunner;

        public TableEngine()
        {
            _scriptRepository = new TableScriptRepository();
            _tableAnalyzer = new TableAnalyzer();
            _runtimeRepository = new TableRuntimeRepository(_scriptRepository, _tableAnalyzer);
            _tableRunner = new TableRunner();

            _scriptRepository.OnTableCreated += id => { _runtimeRepository.InvalidateTable(id); };
            _scriptRepository.OnTableRemoved += id => { _tableAnalyzer.RemoveTable(id); };
            _scriptRepository.OnParentUpdate += (id, parent, newParent) =>
            {
                // TODO
            };
            _scriptRepository.OnPropertyUpdated += desc =>
            {
                _tableAnalyzer.UpdateProperty(desc);
                _runtimeRepository.UpdateProperty(desc);
            };
            _scriptRepository.OnPropertyRemoved += desc =>
            {
                _tableAnalyzer.RemoveProperty(desc);
                _runtimeRepository.RemoveProperty(desc);
            };
        }
    }
}