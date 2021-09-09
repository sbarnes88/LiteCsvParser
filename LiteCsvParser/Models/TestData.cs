using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteCsvParser.Models
{
    public class TestData
    {
        [CsvColumn("first_name", true, MinimumLength = 0, MaximumLength = 20)]
        public string FirstName { get; set; }
        [CsvColumn("last_name", true, MinimumLength = 0, MaximumLength = 20)]
        public string LastName { get; set; }
        [CsvColumn("address", true, MaximumLength = 30)]
        public string Address { get; set; }
        [CsvColumn("city", true)]
        public string City { get; set; }
        [CsvColumn("state", true, MinimumLength = 2, MaximumLength = 2)]
        public string StateCode { get; set; }
        [CsvColumn("zip", true, MinimumLength = 5, MaximumLength =9)]
        public string ZipCode { get; set; }
        [CsvColumn("married", true, trueBoolValues:new[] { "Y", "yes", "y" })]
        public bool Married { get; set; }
    }
}
