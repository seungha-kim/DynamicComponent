using Formula.Interface;
using Formula.ValueRepresentation;

namespace Formula.TestUtil
{
    public class DummyEvaluationContext : IEvaluationContext
    {
        public FormulaValue LookupIdentifier(string name)
        {
            return FormulaValue.NullValue;
        }

        public FormulaValue LookupProperty(string receiver, string name)
        {
            return FormulaValue.NullValue;
        }
    }
}