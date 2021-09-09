using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using LiteCsvParser.Handlers;
using LiteCsvParser.Models;

namespace LiteCsvParser
{
    public class App
    {
        public App()
        {
            var csv = new CsvEngine<TestData>();
            csv.Validate += CsvOnValidateData;
            var data = csv.ReadCsvRemoveDuplicates<TestData, string>("test.csv", k => k.LastName);
            
            //var data = csv.ReadCsv<TestData>("test.csv");
            
            Console.WriteLine("Out of {0} records only {1} could be processed.", csv.TotalRows, data.Count);
            Console.ReadLine();
        }

        private void CsvOnValidateData(object sender, TestData testData)
        {
            if (string.IsNullOrWhiteSpace(testData.LastName))
                return;

            if (testData.LastName.Equals("Doe", StringComparison.InvariantCultureIgnoreCase))
                throw new Exception("Cannot use the last name [DOE]");
        }

        public void Run()
        {
            
        }
    }
}
