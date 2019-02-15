using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Utils
{
    /// <summary>
    /// 当需要存储有大量连续相同的数字时，用这个数据结构可以节省内存。
    /// </summary>
    public sealed class ValueSequence : IEnumerable<Double>
    {
        private class ValueCollection
        {
            public Double Value { get; set; }
            public Int32 Count { get; set; }
            public Double Sum
            {
                get { return Value * Count; }
            }

            public override string ToString()
            {
                return String.Format("{0} x {1}", Value, Count);
            }
        }

        #region Properties
        public Int32 Count
        {
            get { return _count; }
        }
        #endregion

        #region Constructor
        public ValueSequence()
        {
            _sequence = new LinkedList<ValueCollection>();
            _count = 0;
        } 

        public ValueSequence(IEnumerable<Double> initialValues)
            : this()
        {
            AddRange(initialValues);
        }
        #endregion

        #region Value getters
        public Double this[Int32 index]
        {
            get
            {
                VerifyIndexRange(index);
                if (index == 0)
                {
                    return 0.0;
                }
                LinkedListNode<ValueCollection> curNode = _sequence.First;
                while (curNode != null)
                {
                    var collection = curNode.Value;
                    if (collection.Count <= index)
                    {
                        index -= collection.Count;
                    }
                    else
                    {
                        return collection.Value;
                    }
                    curNode = curNode.Next;
                }
                return 0.0;
            }
        }

        public Double GetSumValue(Int32 index)
        {
            VerifyIndexRange(index);
            if (index == 0)
            {
                return 0.0;
            }

            Double ret = 0.0;
            LinkedListNode<ValueCollection> curNode = _sequence.First;
            while (curNode != null)
            {
                if (curNode.Value.Count <= index)
                {
                    index -= curNode.Value.Count;
                    ret += curNode.Value.Sum;
                }
                else
                {
                    ret += index * curNode.Value.Value;
                    break;
                }
                curNode = curNode.Next;
            }
            return ret;
        } 
        #endregion

        #region Collection operations
        public void Add(Double value)
        {
            AddRange(new Double[] { value, });
        }

        public void AddRange(IEnumerable<Double> values)
        {
            foreach (Double value in values)
            {
                if (_sequence.Count <= 0)
                {
                    _sequence.AddLast(new ValueCollection { Value = value, Count = 1, });
                    ++_count;
                    continue;
                }
                ValueCollection collection = _sequence.Last.Value;
                if (IsClose(collection.Value, value))
                {
                    ++collection.Count;
                }
                else
                {
                    collection = new ValueCollection { Value = value, Count = 1, };
                    _sequence.AddLast(collection);
                }
                ++_count;
            }
        }

        public void Insert(Int32 index, Double value)
        {
            InsertRange(index, new Double[] { value, });
        }

        public void InsertRange(Int32 index, IEnumerable<Double> values)
        {
            VerifyIndexRange(index);
            LinkedListNode<ValueCollection> curNode = _sequence.First;
            while (curNode != null)
            {
                ValueCollection collection = curNode.Value;
                if (collection.Count <= index)
                {
                    index -= collection.Count;
                    curNode = curNode.Next;
                }
                else if (curNode == null)
                {
                    AddRange(values);
                    break;
                }
                else
                {
                    // 倒序插入
                    foreach (Double value in values.Reverse())
                    {
                        if (IsClose(collection.Value, value))
                        {
                            // 匹配，无需拆分
                            ++collection.Count;
                        }
                        else
                        {
                            if (index > 0)
                            {
                                // 当前collection需要拆成两个，并且中间插入新的collection
                                Int32 seperateCnt = collection.Count - index;
                                collection.Count = index;
                                _sequence.AddAfter(curNode, new ValueCollection { Value = collection.Value, Count = seperateCnt, });
                                collection = new ValueCollection { Value = value, Count = 1, };
                                _sequence.AddAfter(curNode, collection);
                                curNode = curNode.Next;
                                index = 0;
                            }
                            else
                            {
                                collection = new ValueCollection { Value = value, Count = 1, };
                                _sequence.AddBefore(curNode, collection);
                                curNode = curNode.Previous;
                            }
                        }
                        ++_count;
                    }
                    // prev和current可能需要合并
                    if (curNode.Previous != null && IsClose(curNode.Value.Value, curNode.Previous.Value.Value))
                    {
                        curNode.Previous.Value.Count += curNode.Value.Count;
                        _sequence.Remove(curNode);
                    }
                    break;
                }
            }
        }

        public void RemoveAt(Int32 index)
        {
            RemoveRange(index, 1);
        }

        public void RemoveRange(Int32 index, Int32 length)
        {
            VerifyIndexRange(index);
            if (_count == 0 || index == _count)
            {
                return;
            }
            length = Math.Min(length, _count - index);
            if (index == 0 && length == _count)
            {
                Clear();
                return;
            }
            LinkedListNode<ValueCollection> curNode = _sequence.First;
            while (curNode != null)
            {
                var collection = curNode.Value;
                if (collection.Count <= index)
                {
                    index -= collection.Count;
                }
                else
                {
                    break;
                }
                curNode = curNode.Next;
            }

            _count -= length;
            var nodesToRemove = new List<LinkedListNode<ValueCollection>>();
            while (curNode != null)
            {
                if (length <= curNode.Value.Count - index)
                {
                    curNode.Value.Count -= length;
                    if (curNode.Value.Count == 0)
                    {
                        nodesToRemove.Add(curNode);
                    }
                    break;
                }
                else
                {
                    length -= curNode.Value.Count - index;
                    curNode.Value.Count = index;
                    index = 0;
                    if (curNode.Value.Count == 0)
                    {
                        nodesToRemove.Add(curNode);
                    }
                    curNode = curNode.Next;
                }
            }
            Merge(nodesToRemove);
        }

        public void Clear()
        {
            _sequence.Clear();
            _count = 0;
        }
        #endregion

        #region Private
        private void Merge(LinkedListNode<ValueCollection> node)
        {
            var prev = node.Previous;
            var next = node.Next;
            _sequence.Remove(node);
            if (prev == null || next == null)
            {
                return;
            }
            if (IsClose(prev.Value.Value, next.Value.Value))
            {
                // 合并前后节点
                prev.Value.Count += next.Value.Count;
                _sequence.Remove(next);
            }
        }

        private void Merge(List<LinkedListNode<ValueCollection>> nodes)
        {
            if (nodes.Count <= 0)
            {
                return;
            }
            if (nodes.Count == 1)
            {
                Merge(nodes[0]);
                return;
            }
            var prev = nodes[0].Previous;
            var next = nodes[nodes.Count - 1].Next;
            foreach (var node in nodes)
            {
                _sequence.Remove(node);
            }
            if (prev == null || next == null)
            {
                return;
            }
            if (IsClose(prev.Value.Value, next.Value.Value))
            {
                // 合并前后节点
                prev.Value.Count += next.Value.Count;
                _sequence.Remove(next);
            }
        }

        private Boolean IsClose(Double val1, Double val2)
        {
            return Math.Abs(val1 - val2) < 0.00001;
        }

        private void VerifyIndexRange(Int32 index)
        {
            if (index < 0 || index > _count)
            {
                throw new ArgumentOutOfRangeException("index", String.Format("0 <= index <= {0}", _count));
            }
        } 
        #endregion

        #region IEnumerable<Double>
        public IEnumerator<double> GetEnumerator()
        {
            foreach (ValueCollection collection in _sequence)
            {
                Int32 idx = 0;
                while (idx < collection.Count)
                {
                    yield return collection.Value;
                    ++idx;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(String.Format("Count: {0}", _count));
            Int32 sum = 0;
            foreach (var collection in _sequence)
            {
                sb.AppendLine(collection.ToString());
                sum += collection.Count;
            }
            sb.AppendLine(String.Format("Count: {0}", sum));
            return sb.ToString();
        }
        #endregion

        private LinkedList<ValueCollection> _sequence;
        private Int32 _count;
    }
}
