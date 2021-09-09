using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteCsvParser.Models
{
    public class CsvColumnAttribute : Attribute
    {
        public string ColumnName;
        public bool CanBeNull;
        public int MinimumLength = 0;
        public int MaximumLength = Int32.MaxValue;
        public string[] BoolValues;

        public CsvColumnAttribute(string columnName, bool canBeNull = false, string[] trueBoolValues = null)
        {
            if (trueBoolValues == null)
                BoolValues = new string[1] { "Y" };
            ColumnName = columnName;
            CanBeNull = canBeNull;
        }
    }
}
