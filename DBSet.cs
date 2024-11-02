using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public class DBSet<T> : IEnumerable where T : new()
    {
        private List<string> sqlCommend = new List<string>();
        private SqlConnection conn;

        public DBSet(SqlConnection conn)
        {
            var props = typeof(T).GetProperties();
            string fieldName = "";
            foreach (var prop in props)
            {
                fieldName += $"{prop.Name},";
            }
            fieldName = fieldName.TrimEnd(',');
            sqlCommend.Add($"Select {fieldName} from {typeof(T).Name} where ");
            this.conn = conn;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            string sql = string.Join("", sqlCommend);
            var cmd = new SqlCommand(sql, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            return new SQLEnumerator<T>(reader);
        }

        public DBSet<T> Where(Expression<Func<T, bool>> expression )
        {
            if(sqlCommend.Count > 1)
                sqlCommend.Add(" and ");
            var sql = new SQLExpressionVisitor(new List<string>()).Compile(expression);
            sqlCommend.AddRange(sql);
            return this;
        }

        public IEnumerable<TResult> Select<TResult>(Expression<Func<T, TResult>> selectExpression)
        {
            var func = selectExpression.Compile(); //expression 解開形成 Func<T,TResult>
            foreach (T item in this)
            {
                var result = func.Invoke(item); //每筆呼叫 func 傳回 TResult
                yield return result;
            }
            yield break;
        }
    }
}
