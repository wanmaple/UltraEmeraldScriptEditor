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
    public sealed class AutoFilterObservableCollection<T> : ICollection<T>, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged where T : new()
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
                VerifyIndexRange(index);
                return _innerCollection[_orders.ElementAt(index)];
            }
            set
            {
                VerifyIndexRange(index);
                _innerCollection[_orders.ElementAt(index)] = value;
            }
        }

        public void Add(T item)
        {
            if (Contains(item))
            {
                throw new InvalidOperationException(String.Format("{0} already exists.", item));
            }
            Int32 index = _innerCollection.IndexOf(item);
            if (index >= 0)
            {
                _orders.AddLast(index);
                ++_count;
            }
        }

        public void Clear()
        {
            _count = 0;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < _count; i++)
            {
                Int32 idx = _orders.ElementAt(i);
                array[arrayIndex + i] = _innerCollection[idx];
            }
        }

        public bool Remove(T item)
        {
            Int32 idx = IndexOf(item);
            if (idx < 0)
            {
                return false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                Int32 index = _orders.ElementAt(i);
                yield return _innerCollection[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                Int32 idx = _orders.ElementAt(i);
                if (_innerCollection[idx].Equals(item))
                {
                    return i;
                }
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException("Insert is not supported in AutoFilterObservableCollection. Use Insert of inner collection instead.");
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

        private void VerifyIndexRange(Int32 index)
        {
            if (index < 0 || index >= _count)
            {
                throw new ArgumentOutOfRangeException("0 <= index < " + _count.ToString());
            }
        }

        private ObservableCollection<T> _innerCollection;
        private LinkedList<Int32> _orders;
        private Int32 _count;
    }
}
