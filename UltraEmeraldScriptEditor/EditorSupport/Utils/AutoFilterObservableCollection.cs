using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
    public sealed class AutoFilterObservableCollection<T> : ICollection<T>, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        public AutoFilterObservableCollection(ObservableCollection<T> innerCollection)
        {
            _innerCollection = innerCollection ?? throw new ArgumentNullException("innerCollection");
            _innerCollection.CollectionChanged += OnCollectionChanged;
            _count = _innerCollection.Count;
            Int32 idx = 0;
            foreach (var item in _innerCollection)
            {
                _orders.AddLast(idx);
                ++idx;
            }
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException("0 <= index < " + _count.ToString());
                }
                return _innerCollection[_orders.ElementAt(index)];
            }
            set
            {
                if (index < 0 || index >= _count)
                {
                    throw new ArgumentOutOfRangeException("0 <= index < " + _count.ToString());
                }
                _innerCollection[_orders.ElementAt(index)] = value;
            }
        }

        public void Add(T item)
        {
            _innerCollection.Add(item);
            ++_count;
        }

        public void Clear()
        {
            _innerCollection.Clear();
        }

        public bool Contains(T item)
        {
            return _innerCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _innerCollection.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _innerCollection.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _orders.Count; i++)
            {
                Int32 index = _orders[i];
                yield return _innerCollection[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return _innerCollection.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _innerCollection.Insert(_orders[index], item);
        }

        public void RemoveAt(int index)
        {
            _innerCollection.RemoveAt(index);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        private ObservableCollection<T> _innerCollection;
        private LinkedList<Int32> _orders;
        private Int32 _count;
    }
}
