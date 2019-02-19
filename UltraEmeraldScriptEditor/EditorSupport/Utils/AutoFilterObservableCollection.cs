using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace EditorSupport.Utils
{
    /// <summary>
    /// 支持自动过滤的ObservableCollection。
    /// </summary>
    /// <remarks>
    /// 为了避免排序带来的性能消耗，
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class AutoFilterObservableCollection<T> : Collection<T> //INotifyCollectionChanged
    {

    }
}
