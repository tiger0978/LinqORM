using SQL_Connection;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    internal class DatabaseContext
    {
        private SqlConnection conn { get; set; }

        public DBSet<Employee> Employee { get; set; }
        public DatabaseContext(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
            Employee = new DBSet<Employee>(conn);
        }

        public List<T> QueryData<T>(string sqlcommend) where T : new()
        {
            var cmd = new SqlCommand(sqlcommend, conn);
            SqlDataReader reader = cmd.ExecuteReader();

            List<T> datas = new List<T>();
            while (reader.Read())
            {
                T t = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var prop = t.GetType().GetProperty(reader.GetName(i));
                    prop.SetValue(t, reader.GetValue(i));
                }
                datas.Add(t);
            }
            reader.Close();
            return datas;
        }

        public T QueryFirstorDefault<T>(string sqlcommend) where T : new()
        {
            var cmd = new SqlCommand(sqlcommend, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            T t = new T();
            if (reader.Read())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var prop = t.GetType().GetProperty(reader.GetName(i));
                    prop.SetValue(t, reader.GetValue(i));
                }
            }
            reader.Close();
            return t;
        }

        public int ExecuteNonQuery(string sqlcommend)
        {
            var cmd = new SqlCommand(sqlcommend, conn);
            int count = cmd.ExecuteNonQuery();
            return count;
        }

        public List<T> Where<T>(Expression<Func<T, bool>> expression) where T : new()
        {
            //Select(x=> new Test(x.Name,x.Address))
            StringBuilder sqlCommend = new StringBuilder();
            var props = typeof(T).GetProperties();
            string fieldName = "";
            foreach(var prop in props)
            {
                fieldName += $"{prop.Name},";
            }
            fieldName = fieldName.TrimEnd(',');
            sqlCommend.Append($"Select {fieldName} from {typeof(T).Name} where ");
            // Expression 父類別
            // MemberExpression => 紀錄當前欄位
            // BinaryExpression => 紀錄多條件
            // MethodCallExpression => 紀錄呼叫的函數
            // ContantExpression => 常數 (Leo/30) 
            ConvertExpressionToSqlCommend(sqlCommend, expression.Body);
            var cmd = new SqlCommand(sqlCommend.ToString(), conn);
            SqlDataReader reader = cmd.ExecuteReader();

            List<T> datas = new List<T>();
            while (reader.Read())
            {
                T t = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var prop = t.GetType().GetProperty(reader.GetName(i));
                    if (prop != null)
                    {
                        prop.SetValue(t, reader.GetValue(i));
                    }
                }
                datas.Add(t);
            }
            reader.Close();
            return datas;
        }

        public List<TResult> Select<T,TResult>(List<T> source, Expression<Func<T, TResult>> selectExpression) where T : new()
        {
            List<TResult> results = new List<TResult>();
            var func = selectExpression.Compile(); //expression 解開形成 Func<T,TResult>
            foreach(var item in source) 
            {
               var result = func.Invoke(item); //每筆呼叫 func 傳回 TResult
               results.Add(result);
            }
            return results;
        }

        public void ConvertExpressionToSqlCommend(StringBuilder sqlCommend, Expression expression)
        {
            //sqlCommend = new SQLExpressionVisitor(sqlCommend).Compile(expression);
            //sqlCommend = new SQLExpressionVisitor(sqlCommend).Compile(expression);
            //if (expression is BinaryExpression binary)
            //{
            //    ConvertExpressionToSqlCommend(sqlCommend, binary.Left);
            //    ConvertExpressionNodeType(sqlCommend, binary.NodeType);
            //    ConvertExpressionToSqlCommend(sqlCommend, binary.Right);
            //    if (binary.NodeType == ExpressionType.OrElse)
            //        sqlCommend.Append(") ");
            //}
            //else if (expression is MemberExpression member)
            //{
            //    sqlCommend.Append(member.Member.Name);
            //    ConvertExpressionNodeType(sqlCommend, expression.NodeType);
            //}
            //else if (expression is ConstantExpression constant)
            //{
            //    sqlCommend.Append($"'{constant.Value}'");
            //}
            //else if (expression is MethodCallExpression callExpression)
            //{
            //    switch (callExpression.Method.Name)
            //    {
            //        case "Parse":
            //            if (callExpression.Method.ReturnType.Name == "Guid")
            //            {
            //                var data = callExpression.Arguments[0] as ConstantExpression;
            //                var result = Guid.Parse(data.Value.ToString());
            //                sqlCommend.Append(result);
            //            }
            //            break;
            //        case "Contains":
            //            var arg = callExpression.Arguments[0] as ConstantExpression;
            //            var arg_value = arg.Value.ToString();
            //            var memberExpression = callExpression.Object as MemberExpression;
            //            var fieldName = memberExpression.Member.Name;
            //            sqlCommend.Append($"{fieldName} like '%{arg_value}%'");
            //            break;
            //    }
            //}
        }

        public int DeleteData(object data)
        {
            string tableName = data.GetType().Name;
            var props = data.GetType().GetProperties().Select(x => x.Name + "=" + "\'" + x.GetValue(data).ToString() + "\'").ToArray();
            string delete_datas = String.Join(" and ", props);
            string queryPKcommend = $"select COLUMN_NAME as PK from INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
                $" where SUBSTRING(CONSTRAINT_NAME, 1, 2) = 'PK' and TABLE_NAME = '{tableName}'";
            var cmd = new SqlCommand(queryPKcommend, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            string PK_name = "";
            if (reader.Read())
            {
                PK_name = reader[0].ToString();
            }
            reader.Close();
            string PK_value = data.GetType().GetProperty(PK_name).GetValue(data).ToString();
            string sqlCommend = $"Delete from {tableName} where {PK_name} = '{PK_value}'";
            cmd.CommandText = sqlCommend;
            int count = cmd.ExecuteNonQuery();
            return count;
        }
        //public int UpdateData(object data)
        //{
        //    var tableName = data.GetType().Name;
        //    string queryPKcommend = $"select COLUMN_NAME as PK from INFORMATION_SCHEMA.KEY_COLUMN_USAGE" +
        //        $" where SUBSTRING(CONSTRAINT_NAME, 1, 2) = 'PK' and TABLE_NAME = '{tableName}'";
        //    var cmd = new SqlCommand(queryPKcommend, conn);
        //    SqlDataReader reader = cmd.ExecuteReader();
        //    string PK_name = "";
        //    if (reader.Read())
        //    {
        //        PK_name = reader[0].ToString();
        //    }
        //    reader.Close();
        //    string PK_value = data.GetType().GetProperty(PK_name).GetValue(data).ToString();
        //    var props = data.GetType().GetProperties();
        //    string updateDataCommend = "";
        //    foreach (var prop in props)
        //    {
        //        if (prop.Name == PK_name || prop.GetValue(data) == null)
        //        {
        //            continue;
        //        }
        //        updateDataCommend += prop.Name + "=" + "\'" + prop.GetValue(data).ToString() + "\' ,";
        //    }
        //    updateDataCommend = updateDataCommend.TrimEnd(',');
        //    string sqlCommend = $"Update {tableName} set {updateDataCommend} where {PK_name} = '{PK_value}'";
        //    cmd.CommandText = sqlCommend;
        //    int count = cmd.ExecuteNonQuery();
        //    return count;
        //}
    }
}
