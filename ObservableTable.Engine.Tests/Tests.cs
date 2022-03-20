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

            var scriptParent = ScriptRepository.CreateTableScript(new TableId("idParent"), "parent");
            scriptParent.UpdatePropertyFormula("a", "2");

            var scriptChild1 = ScriptRepository.CreateTableScript(new TableId("idChild1"), "child1");
            scriptChild1.UpdatePropertyFormula("x", "y");
            scriptChild1.UpdatePropertyFormula("y", "1");
            scriptChild1.UpdatePropertyFormula("z", "parent!a + y");
            scriptChild1.UpdatePropertyFormula("w", "z");
            scriptChild1.ParentId = scriptParent.ID;

            Run();
        }

        internal void Run(bool keepInternalState = false)
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

            if (!keepInternalState)
            {
                ModificationSummary.Clear();
            }
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
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("idChild1"), "x")).ToList();
                Assert.Equal(references.Count, 1);
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("idChild1"), "y")));
            }

            {
                // y -> ?
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("idChild1"), "y")).ToList();
                Assert.Equal(references.Count, 0);
            }

            {
                // z -> parent!a, y
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("idChild1"), "z")).ToList();
                Assert.Equal(references.Count, 2);
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("idChild1"), "y")));
                Assert.True(references.Contains(new PropertyDescriptor(new TableId("idParent"), "a")));
            }

            {
                // a -> ?
                var references = analysis.GetReferences(new PropertyDescriptor(new TableId("idParent"), "a")).ToList();
                Assert.Equal(references.Count, 0);
            }

            {
                // parent!a <- z
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("idParent"), "a")).ToList();
                Assert.Equal(observers.Count, 1);
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("idChild1"), "z")));
            }

            {
                // z <- w
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("idChild1"), "z")).ToList();
                Assert.Equal(observers.Count, 1);
            }

            {
                // w <- ?
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("idChild1"), "w")).ToList();
                Assert.Equal(observers.Count, 0);
            }

            {
                // y <- x, z
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("idChild1"), "y")).ToList();
                Assert.Equal(observers.Count, 2);
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("idChild1"), "x")));
                Assert.True(observers.Contains(new PropertyDescriptor(new TableId("idChild1"), "z")));
            }

            {
                // x <- ?
                var observers = analysis.GetObservers(new PropertyDescriptor(new TableId("idChild1"), "x")).ToList();
                Assert.Equal(observers.Count, 0);
            }
        }

        [Fact]
        public void TestSimpleCase_TableRunner()
        {
            var testCase = new BasicTestCase();

            Assert.Equal(FormulaValue.NewNumberValue(1.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("x"));

            Assert.Equal(FormulaValue.NewNumberValue(1.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("y"));

            Assert.Equal(FormulaValue.NewNumberValue(3.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("z"));

            Assert.Equal(FormulaValue.NewNumberValue(2.0f),
                testCase.RuntimeRepository.GetTableById(new TableId("idParent"))?.GetProperty("a"));
        }

        [Fact]
        public void TestSimpleCase_UpdateObservers()
        {
            var tc = new BasicTestCase();

            var scriptChild1 = tc.ScriptRepository.GetTableScript(new TableId("idChild1"));
            scriptChild1.UpdatePropertyFormula("y", "10");

            var scriptParent = tc.ScriptRepository.GetTableScript(new TableId("idParent"));
            scriptParent.UpdatePropertyFormula("a", "20");

            tc.Run();

            Assert.Equal(FormulaValue.NewNumberValue(30.0f),
                tc.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("z"));

            Assert.Equal(FormulaValue.NewNumberValue(30.0f),
                tc.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("w"));
        }

        [Fact]
        public void TestSimpleCase_Cyclic()
        {
            var tc = new BasicTestCase();

            // cycle with length 2
            var scriptChild1 = tc.ScriptRepository.GetTableScript(new TableId("idChild1"));
            scriptChild1.UpdatePropertyFormula("y", "x");

            tc.Run();

            var summary = tc.TableAnalyzer.GetSummary();
            Assert.True(summary.IsCyclic);

            var cycle = summary.GetAllReferenceCycle()!.ToHashSet();
            Assert.Equal(4, cycle.Count);
            Assert.Contains(new PropertyDescriptor(new TableId("idChild1"), "x"), cycle);
            Assert.Contains(new PropertyDescriptor(new TableId("idChild1"), "y"), cycle);
            Assert.Contains(new PropertyDescriptor(new TableId("idChild1"), "z"), cycle);
            Assert.Contains(new PropertyDescriptor(new TableId("idChild1"), "w"), cycle);

            // cycle 해결된 뒤에 다시 Run 실행하면 원상복구되어야 함
            scriptChild1.UpdatePropertyFormula("y", "10");
            tc.Run();
            Assert.False(tc.TableAnalyzer.GetSummary().IsCyclic);
            Assert.Equal(FormulaValue.NewNumberValue(10.0f),
                tc.RuntimeRepository.GetTableById(new TableId("idChild1"))?.GetProperty("x")
            );
        }
    }
}