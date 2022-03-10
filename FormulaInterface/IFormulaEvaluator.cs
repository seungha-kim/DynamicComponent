using FormulaAST;

namespace FormulaInterface
{
    public interface IFormulaEvaluator
    {
        public float EvaluateAsNumber(IEvaluationContext ctx, Expression expr);
        public void EvaluateAsVoid(IEvaluationContext ctx, Expression expr);
    }
}
