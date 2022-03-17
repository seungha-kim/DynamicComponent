using System;
using System.Collections.Generic;

namespace ObservableTable.Engine
{
    public class TableEngine
    {
        private readonly TableScriptRepository _scriptRepository;
        private readonly PropertyExpressionRepository _propertyExpressionRepository;
        private readonly TableRuntimeRepository _runtimeRepository;
        private readonly TableModificationSummary _tableModificationSummary;

        private readonly TableAnalyzer _tableAnalyzer;
        private readonly TableExecutor _tableExecutor;

        public TableEngine()
        {
            _scriptRepository = new TableScriptRepository();
            _propertyExpressionRepository = new PropertyExpressionRepository();
            _tableAnalyzer = new TableAnalyzer();
            _runtimeRepository = new TableRuntimeRepository();
            _tableExecutor = new TableExecutor();
            _tableModificationSummary = new TableModificationSummary();
            _tableModificationSummary.Connect(_scriptRepository);
        }

        public void Update()
        {
            _tableAnalyzer.Update(new TableAnalyzeContext()
            {
                ScriptRepository = _scriptRepository,
                PropertyExpressionRepository = _propertyExpressionRepository,
                TableModificationSummary = _tableModificationSummary,
            });

            var analysisSummary = _tableAnalyzer.GetSummary();

            if (analysisSummary.IsCyclic)
            {
                // TODO: cleanup state
                throw new Exception("TODO: Cyclic reference error");
            }

            _tableExecutor.Execute(new TableExecuteContext()
            {
                PropertyExpressionRepository = _propertyExpressionRepository,
                RuntimeRepository = _runtimeRepository,
                AnalysisSummary = analysisSummary,
            });

            _tableModificationSummary.Clear();
        }
    }
}