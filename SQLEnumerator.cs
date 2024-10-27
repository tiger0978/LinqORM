using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public class SQLEnumerator<T> : IEnumerator where T : new()
    {
        public SqlDataReader reader;

        public object Current
        {
            get
            {
                T t = new T();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var prop = t.GetType().GetProperty(reader.GetName(i));
                    prop.SetValue(t, reader.GetValue(i));
                }
                return t;
            }
        }

        public SQLEnumerator(SqlDataReader reader)
        {
            this.reader = reader;
        }

        public bool MoveNext()
        {
            bool isEnd = reader.Read();
            if (!isEnd)
                reader.Close();
            return isEnd;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
