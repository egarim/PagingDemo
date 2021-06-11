using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PagingDemo
{
    public class Tests
    {
        IDataLayer dl;
        [SetUp]
        public void Setup()
        {
            string conn = @"Integrated Security=SSPI;Pooling=false;Data Source=(localdb)\mssqllocaldb;Initial Catalog=ImportTest";
            dl = XpoDefault.GetDataLayer(conn, DevExpress.Xpo.DB.AutoCreateOption.DatabaseAndSchema);
            using (Session session = new Session(dl))
            {
                Assembly[] assemblies = new System.Reflection.Assembly[] {
                   typeof(Customer).Assembly
                };
                session.UpdateSchema(assemblies);
                session.CreateObjectTypeRecords(assemblies);
            }
        }
        [Test]
        public void CreateData()
        {
            var UoW = new UnitOfWork(dl);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < 10000; i++)
            {
                Customer customer = new Customer(UoW);
                customer.Code = $"CUS{i}";
                customer.Name = $"Customer {i}";
                builder.AppendLine($"{DateTime.Now};{5000.00};CUS{i};INV{i}");
            }
            File.WriteAllText("Data.txt", builder.ToString());
            UoW.CommitChanges();

            Assert.Pass();
        }
       
        [Test]
        public void DirectRead()
        {
            var UoW = new UnitOfWork(dl);
            string aLine = "";
            StringReader strReader = new StringReader(File.ReadAllText("Data.txt"));

            var StartDate = DateTime.Now;
            while (true)
            {
                aLine = strReader.ReadLine();
               
                if (aLine != null)
                {
                    var values = aLine.Split(';');
                    Invoice invoice = new Invoice(UoW);
                    invoice.Date = DateTime.Parse(values[0]);
                    invoice.Total = Decimal.Parse(values[1]);
                    invoice.Customer = UoW.FindObject<Customer>(new BinaryOperator("Code", values[2]));
                    invoice.InvoiceNumber= values[3];
                }
                else
                {

                    break;
                }
            }
            UoW.CommitChanges();
            var EndDate = DateTime.Now;
            var TotalMs = (EndDate - StartDate).TotalMilliseconds;
            Debug.WriteLine(TotalMs);
            //TOTAL MS: 3438.3254
            //TOTAL Preloading(2087.2662)
            Assert.Pass();

            //29357.892
        }

        [Test]
        public void Test1()
        {

            var StartDate = DateTime.Now;
            List<string> Lines = new List<string>();
            string aLine = "";
            StringReader strReader = new StringReader(File.ReadAllText("Data.txt"));
            while (true)
            {
                aLine = strReader.ReadLine();
                if (aLine != null)
                {
                    Lines.Add(aLine);
                }
                else
                {

                    break;
                }
            }
            var LinesCount = Lines.Count();
            //int pageNumber = 0;
            int numberOfObjectsPerPage = 2000;

            var PageTotalDouble = (double)LinesCount / numberOfObjectsPerPage;
            var PageTotal = LinesCount / numberOfObjectsPerPage;

            if ((PageTotalDouble % 1) != 0)
            {
                PageTotal = PageTotal + 1;
            }

            for (int pageNumber = 0; pageNumber < PageTotal; pageNumber++)
            {
                var queryResultPage = Lines
                    .Skip(numberOfObjectsPerPage * pageNumber)
                    .Take(numberOfObjectsPerPage);

                List<string> CustomerCodes = new List<string>();
                foreach (var item in queryResultPage)
                {
                    var values = item.Split(';');
                    CustomerCodes.Add(values[2]);
                }

                InOperator inOperator = new InOperator("Code", CustomerCodes);


                var UoW = new UnitOfWork(dl);
                IEnumerable<Customer> Customers = UoW.GetObjects(UoW.GetClassInfo<Customer>(), inOperator, null, 10000, false, false).Cast<Customer>();
                foreach (var item in queryResultPage)
                {
                    var values = item.Split(';');
                    Invoice invoice = new Invoice(UoW);
                    invoice.Date = DateTime.Parse(values[0]);
                    invoice.Total = Decimal.Parse(values[1]);
                    invoice.Customer = Customers.FirstOrDefault(c => c.Code == values[2]);
                    invoice.InvoiceNumber = values[3];
                }
                UoW.CommitChanges();

            }
          
            var EndDate = DateTime.Now;
            var TotalMs = (EndDate - StartDate).TotalMilliseconds;
            Debug.WriteLine(TotalMs);
            //Console.ReadKey();

            Assert.Pass();
        }
    }
}