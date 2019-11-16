using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VideoPlayerTrimmer.Extensions
{
    public static class ObservableExtensions
    {
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                collection.Add(item);
            }
        } 
    }
}
