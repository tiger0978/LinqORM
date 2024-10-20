using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    public class Test
    {
        public Test(string Id, string name) 
        {
            this.Id = Id;
            this.Name = name;
        }
        public Test() { }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int MonthSalary { get; set; }
    }
}
