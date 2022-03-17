using System;
using System.Linq;
using Xunit;
using ObservableTable.Engine;

namespace ObservableTable.Engine.Tests
{
    public class Tests
    {
        [Fact]
        public void Test1()
        {
            var summary = new TableModificationSummary();
            var exprRepo = new PropertyExpressionRepository();
            var scriptRepo = new TableScriptRepository();
            summary.Connect(scriptRepo);

            var script1 = scriptRepo.CreateTableScript(new TableId("id1"), "name1");
            script1.UpdatePropertyFormula("x", "y");
            script1.UpdatePropertyFormula("y", "1");
            script1.UpdatePropertyFormula("z", "name2!a");
            var script2 = scriptRepo.CreateTableScript(new TableId("id2"), "name2");
            script2.UpdatePropertyFormula("a", "2");

            var analyzer = new TableAnalyzer();
            analyzer.Update(new TableAnalyzeContext()
            {
                ScriptRepository = scriptRepo,
                PropertyExpressionRepository = exprRepo,
                TableModificationSummary = summary
            });
            var analysis = analyzer.GetSummary();

            {
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("id1"), "x"));
                // 아무것도 안들어갔네..
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("id1"), "y")));
            }
        }
    }
}