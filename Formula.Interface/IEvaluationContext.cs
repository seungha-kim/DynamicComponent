using Formula.ValueRepresentation;

namespace Formula.Interface
{
    public interface IEvaluationContext
    {
        FormulaValue GetIdentifierValue(string name);
        FormulaValue GetPropertyValue(string receiver, string name);
    }
}
