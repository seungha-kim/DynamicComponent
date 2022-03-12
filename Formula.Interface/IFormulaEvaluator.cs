using Formula.AST;
using Formula.ValueRepresentation;

namespace Formula.Interface
{
    public interface IFormulaEvaluator
    {
        FormulaValue Evaluate(IEvaluationContext ctx, Expression expr);
    }
}
