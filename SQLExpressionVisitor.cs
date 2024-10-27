using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public class SQLExpressionVisitor : ExpressionVisitor
    {
        private List<string> sqlCommend;

        public SQLExpressionVisitor(List<string> sqlCommend)
        {
            this.sqlCommend = sqlCommend;
        }

        public List<string> Compile(Expression expression)
        {
            Visit(expression);
            return this.sqlCommend;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            sqlCommend.Add("(");
            Visit(node.Left);

            ConvertExpressionNodeType(node.NodeType);

            Visit(node.Right);
            sqlCommend.Add(")");

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            sqlCommend.Add(node.Member.Name);
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Type == typeof(string))
            {
                sqlCommend.Add($"'{node.Value}'");
            }
            else
            {
                sqlCommend.Add(node.Value.ToString());
            }
            return node;
        }
        //protected override Expression VisitMethodCall(MethodCallExpression node)
        //{
        //    switch (node.Method.Name)
        //    {
        //        case "Parse":
        //            if (node.Method.ReturnType.Name == "Guid")
        //            {
        //                var data = node.Arguments[0] as ConstantExpression;
        //                var result = Guid.Parse(data.Value.ToString());
        //                sqlCommend.Append(result);
        //            }
        //            break;
        //        case "Contains":
        //            var arg = node.Arguments[0] as ConstantExpression;
        //            var arg_value = arg.Value.ToString();
        //            var memberExpression = node.Object as MemberExpression;
        //            var fieldName = memberExpression.Member.Name;
        //            sqlCommend.Append($"{fieldName} like '%{arg_value}%'");
        //            break;
        //    }
        //    return node;
        //}


        private void ConvertExpressionNodeType(ExpressionType expressionType)
        {
            switch (expressionType)
            {
                case ExpressionType.AndAlso:
                    sqlCommend.Add(" AND ");
                    break;
                case ExpressionType.OrElse:
                    sqlCommend.Add(" OR ");
                    break;
                case ExpressionType.Equal:
                    sqlCommend.Add(" = ");
                    break;
                case ExpressionType.GreaterThan:
                    sqlCommend.Add(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlCommend.Add(" >= ");
                    break;
                case ExpressionType.LessThan:
                    sqlCommend.Add(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlCommend.Add(" <= ");
                    break;
            }
        }
    }
}