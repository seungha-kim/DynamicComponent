using Formula.AST;

namespace Formula.Interface
{
    public interface IFormulaEvaluator
    {
        public float EvaluateAsNumber(IEvaluationContext ctx, Expression expr);
        public void EvaluateAsVoid(IEvaluationContext ctx, Expression expr);
    }
}
