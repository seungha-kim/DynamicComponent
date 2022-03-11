using Formula.AST;

namespace Formula.Interface
{
    public interface IFormulaParser
    {
        public Expression Parse(IParsingContext ctx, string input);
    }
}