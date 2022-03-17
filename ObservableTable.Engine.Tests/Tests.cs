using System;
using System.Linq;
using Xunit;
using ObservableTable.Engine;

namespace ObservableTable.Engine.Tests
{
    public class Tests
    {
        [Fact]
        public void TestTableAnalyzer_SimpleCase()
        {
            var summary = new TableModificationSummary();
            var exprRepo = new PropertyExpressionRepository();
            var scriptRepo = new TableScriptRepository();
            summary.Connect(scriptRepo);

            var script1 = scriptRepo.CreateTableScript(new TableId("id1"), "name1");
            script1.UpdatePropertyFormula("x", "y");
            script1.UpdatePropertyFormula("y", "1");
            script1.UpdatePropertyFormula("z", "name2!a + y");
            script1.ParentId = new TableId("id2");

            var script2 = scriptRepo.CreateTableScript(new TableId("id2"), "name2");
            script2.UpdatePropertyFormula("a", "2");

            // TODO: 자식 속성도 참조 가능?

            var analyzer = new TableAnalyzer();
            analyzer.Update(new TableAnalyzeContext()
            {
                ScriptRepository = scriptRepo,
                PropertyExpressionRepository = exprRepo,
                TableModificationSummary = summary
            });
            var analysis = analyzer.GetSummary();

            {
                // x -> y
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("id1"), "x")).ToList();
                Assert.Equal(references.Count, 1);
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("id1"), "y")));
            }

            {
                // y -> ?
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("id1"), "y")).ToList();
                Assert.Equal(references.Count, 0);
            }

            {
                // z -> name2!a, y
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("id1"), "z")).ToList();
                Assert.Equal(references.Count, 2);
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("id1"), "y")));
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("id2"), "a")));
            }

            {
                // a -> ?
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("id2"), "a")).ToList();
                Assert.Equal(references.Count, 0);
            }

            {
                // name2!a <- z
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id2"), "a")).ToList();
                Assert.Equal(observers.Count, 1);
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("id1"), "z")));
            }

            {
                // z <- ?
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id1"), "z")).ToList();
                Assert.Equal(observers.Count, 0);
            }

            {
                // y <- x, z
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id1"), "y")).ToList();
                Assert.Equal(observers.Count, 2);
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("id1"), "x")));
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("id1"), "z")));
            }

            {
                // x <- ?
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id1"), "x")).ToList();
                Assert.Equal(observers.Count, 0);
            }
        }
    }
}