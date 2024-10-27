using SQL_Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqORM
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //List<int> ints = new List<int>();
            //ints.Select(x => new Employee()
            //{
            //    EmpId = x.ToString()
            //}).ToList();


            string connection_string = "Persist Security Info=False;Integrated Security=true; Initial Catalog=misDB;Server=LAPTOP-VKA6DFIS\\SQLEXPRESS;Encrypt=True;TrustServerCertificate=true;";
            DatabaseContext databaseContext = new DatabaseContext(connection_string);
            List<EMPModel> datas = databaseContext.Employee.Where(x => x.MonthSalary >= 60000)
                                             .Select(x => new EMPModel()
                                             {
                                                 EmpId = x.EmpId,
                                                 Name = x.EmpName
                                             }).ToList();

            foreach (var data in datas) 
            {
                Console.WriteLine(data.Name);
            }

            //string connection_string = "Persist Security Info=False;Integrated Security=true; Initial Catalog=misDB;Server=LAPTOP-VKA6DFIS\\SQLEXPRESS;Encrypt=True;TrustServerCertificate=true;";
            //DatabaseContext databaseContext = new DatabaseContext(connection_string);
            //var datas = databaseContext.QueryData<Employee, Test>(x => x.MonthSalary >= 60000, x => new Test(x.EmpId, x.EmpName));
            //var datas = databaseContext.Where<Employee>(x => x.MonthSalary >= 60000 && x.JobTitle == "研發副理" && x.EmpName.Contains("黃"));

            //var datas = databaseContext.Employee.Where(x => x.MonthSalary >= 60000).Where(x=>x.)
            Console.ReadKey();
        }
    }
}
