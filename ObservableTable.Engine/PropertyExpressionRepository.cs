using System.Collections.Generic;
using System.Linq;
using Formula.AST;

namespace ObservableTable.Engine
{
    internal class PropertyExpressionRepository
    {
        private readonly Dictionary<PropertyDescriptor, Expression> _propertyExpressions =
            new Dictionary<PropertyDescriptor, Expression>();

        public Expression? GetPropertyExpression(PropertyDescriptor desc)
        {
            return _propertyExpressions[desc];
        }

        internal void SetPropertyExpression(PropertyDescriptor desc, Expression expr)
        {
            _propertyExpressions[desc] = expr;
        }

        public IEnumerable<Expression> GetTablePropertyExpressions(TableId id)
        {
            return from desc in _propertyExpressions.Keys
                where desc.ID == id
                select _propertyExpressions[desc];
        }
    }
}