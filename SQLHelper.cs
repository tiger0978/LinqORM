﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SQL_Connection
{
    internal class SQLHelper
    {
        private SqlConnection conn {  get; set; }
        public SQLHelper(string connectionString)
        {
            conn = new SqlConnection(connectionString);
            conn.Open();
        }
        public List<T> QueryData<T>(string sqlcommend) where T : new()
        {
            var cmd = new SqlCommand(sqlcommend, conn);
            SqlDataReader reader = cmd.ExecuteReader();

            List<T> datas = new List<T>();
            while (reader.Read()) 
            {
                T t = new T();
                for (int i=0; i< reader.FieldCount; i++)
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

        public List<T> QueryData<T>(Expression<Func<T,bool>> expression) where T:new()
        {
            // Expression 父類別
            // MemberExpression => 紀錄當前欄位
            // BinaryExpression => 紀錄多條件
            // MethodCallExpression => 紀錄呼叫的函數
            // ContantExpression => 常數 (Leo/30) 
            StringBuilder sqlCommend = new StringBuilder();
            sqlCommend.Append($"Select * from {typeof(T).Name} where ");

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

        public void ConvertExpressionToSqlCommend(StringBuilder sqlCommend, Expression expression)
        {
            if(expression is BinaryExpression binary)
            {
                ConvertExpressionToSqlCommend(sqlCommend, binary.Left);
                ConvertExpressionNodeType(sqlCommend, binary.NodeType);
                ConvertExpressionToSqlCommend(sqlCommend, binary.Right);
                if(binary.NodeType == ExpressionType.OrElse)
                    sqlCommend.Append(") ");
            }
            else if(expression is MemberExpression member)
            {
                sqlCommend.Append(member.Member.Name);
                ConvertExpressionNodeType(sqlCommend, expression.NodeType);
            }
            else if (expression is ConstantExpression constant)
            {
                sqlCommend.Append($"'{constant.Value}'");
            }
            else if(expression is MethodCallExpression callExpression)
            {
                switch(callExpression.Method.Name) 
                {
                    case "Parse":
                        if (callExpression.Method.ReturnType.Name == "Guid")
                        {
                            var data = callExpression.Arguments[0] as ConstantExpression;
                            var result = Guid.Parse(data.Value.ToString());
                            sqlCommend.Append(result);
                        }
                        break;
                    case "Contains":
                        var arg = callExpression.Arguments[0] as ConstantExpression;
                        var arg_value = arg.Value.ToString();
                        var memberExpression = callExpression.Object as MemberExpression;
                        var fieldName = memberExpression.Member.Name;
                        sqlCommend.Append($"{fieldName} like '%{arg_value}%'");
                        break;
                }

            }
        }

        public void ConvertExpressionNodeType(StringBuilder sqlCommend, ExpressionType expressionType)
        {
            
            switch(expressionType) 
            {
                case ExpressionType.AndAlso:
                    sqlCommend.Append(" and ");
                    break;
                case ExpressionType.OrElse:
                    sqlCommend.Append(" or (");
                    break;
                case ExpressionType.Equal:
                    sqlCommend.Append(" = "); 
                    break;
                case ExpressionType.GreaterThan:
                    sqlCommend.Append(" > ");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sqlCommend.Append(" >= ");
                    break;
                case ExpressionType.LessThan:
                    sqlCommend.Append(" < ");
                    break;
                case ExpressionType.LessThanOrEqual:
                    sqlCommend.Append(" <= ");
                    break;
            }
        }

        public List<dynamic> QueryData(string sqlcommend)
        {
            List<dynamic> datas = new List<dynamic>(); //compiler 延後執行
            var cmd = new SqlCommand(sqlcommend, conn);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var data = new ExpandoObject() as IDictionary<string, object>; 
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    data.Add(reader.GetName(i), reader[i]);
                }
                datas.Add(data);
            }
            reader.Close();
            return datas;
        }

        public int AddData(object data)
        {
            string tableName = data.GetType().Name;
            var props = data.GetType().GetProperties();
            string[] columns = props.Select(x=>x.Name).ToArray();
            string[] values = props.Select(x=>"\'" + x.GetValue(data).ToString() +"\'").ToArray();
            string colume_str = String.Join(",", columns);
            string value_str = String.Join(",", values);
            string sqlCommend = $"Insert into {tableName} ({colume_str}) values ({value_str})";
            var cmd = new SqlCommand(sqlCommend, conn);
            int count = cmd.ExecuteNonQuery();
            return count;
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
            string PK_name="";
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

        public int UpdateData(object data) 
        {
            var tableName = data.GetType().Name;
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
            var props = data.GetType().GetProperties();
            string updateDataCommend = "";
            foreach (var prop in props)
            {
                if(prop.Name == PK_name || prop.GetValue(data) == null)
                {
                    continue;
                }
                updateDataCommend += prop.Name + "=" + "\'" + prop.GetValue(data).ToString() + "\' ,";
            }
            updateDataCommend = updateDataCommend.TrimEnd(',');
            string sqlCommend = $"Update {tableName} set {updateDataCommend} where {PK_name} = '{PK_value}'";
            cmd.CommandText = sqlCommend;
            int count = cmd.ExecuteNonQuery();
            return count;
        }
    }
}
