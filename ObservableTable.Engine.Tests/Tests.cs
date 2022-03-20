using System;
using System.Linq;
using Formula.ValueRepresentation;
using Xunit;
using ObservableTable.Engine;

namespace ObservableTable.Engine.Tests
{
    internal class BasicTestCase
    {
        internal TableModificationSummary ModificationSummary { get; }
        internal TableScriptRepository ScriptRepository { get; }
        internal PropertyExpressionRepository ExpressionRepository { get; }
        internal TableAnalyzer TableAnalyzer { get; }
        internal TableExecutor TableExecutor { get; }
        internal TableRuntimeRepository RuntimeRepository { get; }

        internal BasicTestCase()
        {
            ModificationSummary = new TableModificationSummary();
            ScriptRepository = new TableScriptRepository();
            ExpressionRepository = new PropertyExpressionRepository();
            TableAnalyzer = new TableAnalyzer();
            TableExecutor = new TableExecutor();
            RuntimeRepository = new TableRuntimeRepository();

            ModificationSummary.Connect(ScriptRepository);

            var script1 = ScriptRepository.CreateTableScript(new TableId("id1"), "name1");
            script1.UpdatePropertyFormula("x", "y");
            script1.UpdatePropertyFormula("y", "1");
            script1.UpdatePropertyFormula("z", "name2!a + y");
            script1.UpdatePropertyFormula("w", "z");
            script1.ParentId = new TableId("id2");

            var script2 = ScriptRepository.CreateTableScript(new TableId("id2"), "name2");
            script2.UpdatePropertyFormula("a", "2");

            Run();

            // TODO: cyclic
        }

        internal void Run()
        {
            TableAnalyzer.Update(new TableAnalyzeContext()
            {
                ScriptRepository = ScriptRepository,
                PropertyExpressionRepository = ExpressionRepository,
                TableModificationSummary = ModificationSummary
            });

            TableExecutor.Execute(new TableExecuteContext()
            {
                ScriptRepository = ScriptRepository,
                PropertyExpressionRepository = ExpressionRepository,
                RuntimeRepository = RuntimeRepository,
                AnalysisSummary = TableAnalyzer.GetSummary(),
                TableModificationSummary = ModificationSummary
            });
            ModificationSummary.Clear();
        }
    }

    public class Tests
    {
        [Fact]
        public void TestSimpleCase_TableAnalyzer()
        {
            var testCase = new BasicTestCase();
            var analysis = testCase.TableAnalyzer.GetSummary();

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
                // z <- w
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id1"), "z")).ToList();
                Assert.Equal(observers.Count, 1);
            }

            {
                // w <- ?
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("id1"), "w")).ToList();
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

        [Fact]
        public void TestSimpleCase_TableRunner()
        {
            var testCase = new BasicTestCase();

            Assert.Equal(FormulaValue.NewNumberValue(1.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("id1"))?.GetProperty("x"));

            Assert.Equal(FormulaValue.NewNumberValue(1.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("id1"))?.GetProperty("y"));

            Assert.Equal(FormulaValue.NewNumberValue(3.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("id1"))?.GetProperty("z"));

            Assert.Equal(FormulaValue.NewNumberValue(2.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("id2"))?.GetProperty("a"));
        }

        [Fact]
        public void TestSimpleCase_UpdateObservers()
        {
            var tc = new BasicTestCase();

            var script1 = tc.ScriptRepository.GetTableScript(new TableId("id1"));
            script1.UpdatePropertyFormula("y", "10");

            var script2 = tc.ScriptRepository.GetTableScript(new TableId("id2"));
            script2.UpdatePropertyFormula("a", "20");

            tc.Run();

            Assert.Equal(FormulaValue.NewNumberValue(30.0f),
                tc.RuntimeRepository.GetTableById(new TableId("id1"))?.GetProperty("z"));

            Assert.Equal(FormulaValue.NewNumberValue(30.0f),
                tc.RuntimeRepository.GetTableById(new TableId("id1"))?.GetProperty("w"));
        }
    }
}