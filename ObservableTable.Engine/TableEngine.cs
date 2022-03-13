namespace ObservableTable.Engine
{
    public class TableEngine
    {
        private readonly RelationManager _relationManager;
        private readonly TableRuntimeManager _runtimeManager;
        private readonly TableScriptManager _scriptManager;

        public TableEngine()
        {
            _relationManager = new RelationManager();
            _scriptManager = new TableScriptManager(_relationManager);
            _runtimeManager = new TableRuntimeManager(_scriptManager, _relationManager);

            _scriptManager.OnTableInvalidated += id => _runtimeManager.InvalidateTable(id);
            _scriptManager.OnPropertyInvalidated += desc => _runtimeManager.InvalidateProperty(desc);
        }
    }
}