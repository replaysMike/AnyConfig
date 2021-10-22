using System;
using System.Linq.Expressions;
using TypeSupport.Extensions;

namespace AnyConfig
{
    public static class ExpressionExtensions
    {
        internal static string GetExpressionValue(this Expression<Func<object, object>>[] expressions, string name)
        {
            var val = "";
            foreach (var expression in expressions)
            {
                if (expression.Parameters[0].Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    switch (expression.Body)
                    {
                        case BlockExpression e:
                            break;
                        case DefaultExpression e:
                            break;
                        case DynamicExpression e:
                            break;
                        case MemberExpression e:
                            var m = e.Expression as ConstantExpression;
                            val = m.Value.GetFieldValue(e.Member.Name).ToString();
                            break;
                        case MethodCallExpression e:
                            break;
                        case LambdaExpression e:
                            break;
                        case ConstantExpression e:
                            val = e.Value.ToString().Replace("\"", "");
                            break;
                        case UnaryExpression e:
                            break;
                    }
                    break;
                }
            }
            return val;
        }
    }
}
