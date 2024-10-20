using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public class SQLExpressionVisitor:ExpressionVisitor
    {
        private StringBuilder sqlCommend = new StringBuilder();
        private bool isFirst = true;
        private bool isLast = true;
        private ExpressionType nodeType;

        public SQLExpressionVisitor(StringBuilder sqlCommend)
        {
            this.sqlCommend = sqlCommend;
        }

        public StringBuilder Compile(Expression expression)
        {
            Visit(expression);
            return this.sqlCommend;
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (isFirst) 
            {
                this.nodeType = node.NodeType;
            }
            else
            {
                ConvertExpressionNodeType(node.NodeType);
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            this.sqlCommend.Append(node.Member.Name);
            if (isFirst)
            {
                ConvertExpressionNodeType(this.nodeType);
                isFirst = false;
            }
            ConvertExpressionNodeType(node.NodeType);
            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            this.sqlCommend.Append($"'{node.Value}'");
            return base.VisitConstant(node);
        }

        private void ConvertExpressionNodeType(ExpressionType expressionType)
        {

            switch (expressionType)
            {
                case ExpressionType.AndAlso:
                    this.sqlCommend.Append(" and ");
                    break;
                case ExpressionType.OrElse:
                    this.sqlCommend.Append(" or (");
                    break;
                case ExpressionType.Equal:
                    this.sqlCommend.Append(" = ");
                    break;
                case ExpressionType.GreaterThan:
                    this.sqlCommend.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    this.sqlCommend.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    this.sqlCommend.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    this.sqlCommend.Append(" <= ");
                    break;
            }
        }
    }
}
