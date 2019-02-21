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

        private struct PriorityData
        {
            public Int32 index;
            public Int32 priority;
        }

        public void Filter(Predicate<T> filterFunc, Func<T, Int32> priorityFunc)
        {
            _count = 0;
            _orders.Clear();
            var priorities = new List<PriorityData>();
            Int32 idx = 0;
            foreach (var item in _innerCollection)
            {
                if (filterFunc == null || filterFunc(item))
                {
                    Int32 priority = priorityFunc == null ? 0 : priorityFunc(item);
                    priorities.Add(new PriorityData { index = idx, priority = priority });
                }
                ++idx;
            }
            priorities.Sort((item1, item2) =>
            {
                return item1.priority > item2.priority ? 1 : item1.priority < item2.priority ? -1 : 0;
            });
            foreach (var data in priorities)
            {
                _orders.AddLast(data.index);
                ++_count;
            }
        }

        public void Add(T item)
        {
            if (Contains(item))
            {
                return;
            }
            Int32 index = _innerCollection.IndexOf(item);
            if (index >= 0)
            {
                _orders.AddLast(index);
                ++_count;

                TriggerCollectionChanged();
            }
        }

        public void Clear()
        {
            _count = 0;
            _orders.Clear();

            TriggerCollectionChanged();
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
            LinkedListNode<Int32> curNode = _orders.First;
            while (idx > 0)
            {
                --idx;
                curNode = curNode.Next;
            }
            _orders.Remove(curNode);
            --_count;

            TriggerCollectionChanged();
            return true;
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
            VerifyIndexRange(index);
            if (Contains(item))
            {
                return;
            }
            Int32 idx = _innerCollection.IndexOf(item);
            LinkedListNode<Int32> curNode = _orders.First;
            while (index > 0)
            {
                --index;
                curNode = curNode.Next;
            }
            _orders.AddBefore(curNode, idx);
            ++_count;

            TriggerCollectionChanged();
        }

        public void RemoveAt(int index)
        {
            VerifyIndexRange(index);
            LinkedListNode<Int32> curNode = _orders.First;
            while (index > 0)
            {
                --index;
                curNode = curNode.Next;
            }
            _orders.Remove(curNode);
            --_count;

            TriggerCollectionChanged();
        }

        private void TriggerCollectionChanged()
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
