using FormulaAST;

namespace FormulaInterface
{
    public interface IFormulaParser
    {
        public Expression Parse(IParsingContext ctx, string input);
    }
}