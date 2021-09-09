using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteCsvParser.Handlers
{
    public static class ListExtensions
    {
        public static List<T> RemoveDuplicates<T,R>(this List<T> item, Func<T, R> selector, bool selectFirstItem = true) where T : new()
        {
            return selectFirstItem 
                ? item.GroupBy(selector).Select(k => k.First()).ToList() 
                : item.GroupBy(selector).Select(k => k.Last()).ToList();
        }
    }
}
