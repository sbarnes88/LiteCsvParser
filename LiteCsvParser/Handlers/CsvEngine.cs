using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LiteCsvParser.Models;

namespace LiteCsvParser.Handlers
{
    public class CsvEngine<T> where T : new()
    {
        public event EventHandler<T> Validate;

        public int TotalRows { get; set; }

        private Dictionary<int, CsvColumnAttribute> Columns { get; set; }
        private Dictionary<int, string> PropertyNames { get; set; }
        private Dictionary<int, string> Errors { get; set; }

        private int CurrentLineIndex { get; set; }
        private int CurrentColumnIndex { get; set; }

        private List<string> _errorMessages;
        public List<string> ErrorMessages
        {
            get
            {
                _errorMessages.Clear();
                foreach (var error in Errors)
                {
                    _errorMessages.Add(string.Format("Line {0} contained the following errors: {1}", error.Key + 1, error.Value));
                }
                return _errorMessages;
            }
            set { _errorMessages = value; }
        }

        public CsvEngine()
        {
            Columns = new Dictionary<int, CsvColumnAttribute>();
            PropertyNames = new Dictionary<int, string>();
            Errors = new Dictionary<int, string>();
            ErrorMessages = new List<string>();
        }

        public List<T> ReadCsv(string filename)
        {
            var elements = new List<T>();

            using (var stream = new StreamReader(filename))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (TotalRows == 0 && !string.IsNullOrWhiteSpace(line))
                        MapFields<T>(line.Split(',').ToList());
                    if (!string.IsNullOrWhiteSpace(line) && TotalRows > 0)
                        MapLineToType(line.Split(','), elements, TotalRows);
                    TotalRows++;
                    CurrentLineIndex = TotalRows;
                }
            }
            return elements;
        }

        public List<T> ReadCsvRemoveDuplicates<T,R>(string filename, Func<T, R> selector) where T : new()
        {
            var elements = new List<T>();
            
            using (var stream = new StreamReader(filename))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (TotalRows == 0 && !string.IsNullOrWhiteSpace(line))
                        MapFields<T>(line.Split(',').ToList());
                    if (!string.IsNullOrWhiteSpace(line) && TotalRows > 0)
                        MapLineToType(line.Split(','), elements, TotalRows);
                    TotalRows++;
                    CurrentLineIndex = TotalRows;
                    if (TotalRows % 10000 == 0)
                    {
                        elements = elements.RemoveDuplicates(selector);
                        GC.Collect();
                    }
                }
            }
            return elements.RemoveDuplicates(selector);
        }

        protected virtual void OnValidate(T e)
        {
            Validate?.Invoke(this, e);
        }
        
        private void MapLineToType<T>(string[] split, List<T> elements, int index) where T : new()
        {
            var e = new T();
            var innerIndex = -1;

            foreach (var item in split)
            {
                innerIndex++;
                if (!Columns.ContainsKey(innerIndex))
                    continue;

                if (MatchesRules<T>(item, innerIndex, index))
                {
                    MapItem<T>(item, innerIndex, e);
                }

                CurrentColumnIndex = innerIndex;
            }

            ProcessValidation(e);

            if (Errors.ContainsKey(index))
                return;
            elements.Add(e);
        }

        private void ProcessValidation(object element)
        {
            DoValidation((T)element, OnValidate);
        }

        private void DoValidation(T e, Action<T> evt)
        {
            try
            {
                if (e == null)
                    return;
                evt?.Invoke(e);
            }
            catch (Exception ex)
            {
                if (!Errors.ContainsKey(CurrentLineIndex))
                    Errors.Add(CurrentLineIndex, ex.Message);
                else
                    Errors[CurrentLineIndex] += ex.Message;
            }
        }


        protected void AddError<T>(string item, int index, int innerIndex, string message) where T : new()
        {
            if (Errors.ContainsKey(index))
                Errors[index] += string.Format("[{0}] {1} Value: {2} ", Columns[innerIndex].ColumnName, message, item);
            else
                Errors.Add(index, string.Format("[{0}] {1} Value: {2} ", Columns[innerIndex].ColumnName, message, item));
        }

        private bool MatchesRules<T>(string item, int innerIndex, int lineIndex) where T : new()
        {
            if (string.IsNullOrEmpty(item) && !Columns[innerIndex].CanBeNull)
            {
                AddError<T>(item, lineIndex, innerIndex, "was null!");
                return false;
            }

            if ((Columns[innerIndex].MinimumLength == 0 || Columns[innerIndex].MaximumLength == 0) && item == null)
                return true;

            if (item == null)
                return true;
            if (Columns.ContainsKey(innerIndex) && item.Length < Columns[innerIndex].MinimumLength)
            {
                AddError<T>(item, lineIndex, innerIndex, string.Format("is less than the length of {0}", Columns[innerIndex].MinimumLength));
                return false;
            }

            if (Columns.ContainsKey(innerIndex) && item.Length > Columns[innerIndex].MaximumLength)
            {
                AddError<T>(item, lineIndex, innerIndex, string.Format("is greater than the length of {0}", Columns[innerIndex].MaximumLength));
                return false;
            }
            return true;
        }

        private void MapItem<T>(string item, int innerIndex, T element) where T : new()
        {
            // Get the PropertyInfo object:
            var propertyInfo = element.GetType().GetProperty(PropertyNames[innerIndex]);

            if (propertyInfo.PropertyType == typeof(bool))
            {
                var keys = Columns[innerIndex].BoolValues;
                if (keys != null && keys.Contains(item))
                    propertyInfo.SetValue(element, true);
                else
                    propertyInfo.SetValue(element, false);
            }
            else
            {

                if (string.IsNullOrWhiteSpace(item) && Columns.ContainsKey(innerIndex) && Columns[innerIndex].CanBeNull)
                    propertyInfo.SetValue(element, null, null);
                else
                    propertyInfo.SetValue(element, Convert.ChangeType(item, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType));
            }
        }

        private void MapFields<T>(List<string> columns) where T : new()
        {
            var type = typeof(T);

            // Get the PropertyInfo object:
            var properties = type.GetProperties();
            Console.WriteLine("Finding PK for {0}", type.Name);
            foreach (var property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                foreach (var attribute in attributes)
                {
                    if (attribute.GetType() == typeof(CsvColumnAttribute))
                    {
                        var element = attribute as CsvColumnAttribute;
                        if (element == null)
                            continue;
                        var index = columns.IndexOf(element.ColumnName);
                        if (index == -1)
                            continue;
                        PropertyNames.Add(index, property.Name);

                        if (element.MaximumLength < element.MinimumLength || element.MinimumLength < 0 || element.MaximumLength < 0)
                            throw new Exception(string.Format("{0} maximum must be greater than or equal than the minimum and cannot be negative."));

                        var column = new CsvColumnAttribute(element.ColumnName, element.CanBeNull)
                        {
                            MaximumLength = element.MaximumLength,
                            MinimumLength = element.MinimumLength
                        };
                        Columns.Add(index, column);
                    }
                }
            }
        }

    }
}
