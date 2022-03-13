using Formula.Interface;
using Formula.ValueRepresentation;

namespace Formula.TestUtil
{
    public class DummyEvaluationContext : IEvaluationContext
    {
        public FormulaValue GetIdentifierValue(string name)
        {
            return FormulaValue.NullValue;
        }

        public FormulaValue GetPropertyValue(string receiver, string name)
        {
            return FormulaValue.NullValue;
        }
    }
}