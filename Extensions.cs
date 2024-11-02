using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public static class Extensions
    {

        public static List<T> ToList<T>(this IEnumerable<T> source)  
        {
            List<T> results = new List<T>();
            //IEnumerator<T> enumerator = source.GetEnumerator();
            //while (enumerator.MoveNext())
            //{
            //    results.Add(enumerator.Current);
            //}
            foreach (var item in source)
            {
                results.Add(item);
            }
            return results;
        }
    }
}
