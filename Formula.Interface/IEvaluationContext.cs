using Formula.ValueRepresentation;

namespace Formula.Interface
{
    public interface IEvaluationContext
    {
        FormulaValue LookupIdentifier(string name);
        FormulaValue LookupProperty(string receiver, string name);
    }
}
