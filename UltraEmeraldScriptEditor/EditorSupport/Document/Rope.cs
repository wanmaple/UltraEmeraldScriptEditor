using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 插入删除性能较高的List。
    /// </summary>
    /// <remarks>
    /// 由于需要快速插入/删除字符，List的插入和删除性能是满足不了的
    /// 我们用一颗自平衡的二叉树来解决这个问题
    /// 我们将数据只存储到叶子节点中，每个节点记录自己子树的总长度
    /// 靠左的叶子节点的起始偏移小于靠右的叶子节点的起始偏移
    /// 优点在于寻找偏移的时候效率为O(log N)，
    /// 删除数据的时候，只需做少量的数据移动，但是需要合并节点和自平衡
    /// 插入数据的时候，需要将子节点与新的子树重新设置一个父节点，然后自平衡，在插入的数据量比较大时，会经过多次自平衡
    /// 综上所述，对于频繁插入/删除字符，且数据量很大的时候会有比较可观的性能
    /// 线程不安全
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class Rope<T> : IList<T>, ICloneable
    {
        /// <summary>
        /// RopeNode只有叶子结点才负责存储数据，任何高度>0的节点只是负责存储所有子树的总长度
        /// 从左到右依次列出的叶子节点的数据连起来就是根节点所表示的连续数据
        /// </summary>
        [Serializable]
        internal sealed class RopeNode
        {
            internal static readonly Int16 NODE_SIZE = 256;

            internal RopeNode Parent { get; set; }
            internal RopeNode Left { get; set; }
            internal RopeNode Right { get; set; }
            internal Int32 Length { get; set; }
            internal Byte Height { get; set; }
            internal Int32 Balance
            {
                get
                {
                    if (IsLeaf)
                    {
                        return 0;
                    }
                    // RopeNode不可能存在只有一个子节点的情况
                    return Left.Height - Right.Height;
                }
            }
            internal Boolean IsLeaf
            {
                get
                {
                    return Left == null && Right == null;
                }
            }
            internal Boolean IsLeft
            {
                get
                {
                    return Parent != null && Parent.Left == this;
                }
            }
            internal Boolean IsRight
            {
                get
                {
                    return Parent != null && Parent.Right == this;
                }
            }

            public RopeNode()
            {
                Length = 0;
                Height = 0;
                Parent = Left = Right = null;
            }

            internal void GenerateContentsIfRequired()
            {
                if (_contents == null)
                {
                    _contents = new T[NODE_SIZE];
                }
            }

            internal RopeNode Clone()
            {
                var ret = new RopeNode();
                if (Parent != null)
                {
                    ret.Parent = Parent.Clone();
                }
                if (Left != null)
                {
                    ret.Left = Left.Clone();
                }
                if (Right != null)
                {
                    ret.Right = Right.Clone();
                }
                ret.Length = Length;
                ret.Height = Height;
                if (_contents != null)
                {
                    ret.GenerateContentsIfRequired();
                    Array.Copy(_contents, ret._contents, Length);
                }
                return ret;
            }

            internal T[] _contents;
        }

        #region Constructor
        public Rope()
        {
            _root = new RopeNode();
        }
        public Rope(IEnumerable<T> input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }
            _root = new RopeNode();
            AddRange(input.ToArray());
        }
        #endregion

        #region IList<T>
        public T this[int index]
        {
            get
            {
                return InnerGetElement(_root, index);
            }
            set
            {
                InnerSetElement(_root, index, value);
            }
        }

        public int Count
        {
            get { return _root.Length; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            Insert(Count, item);
        }

        public void Clear()
        {
            _root = new RopeNode();
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyTo(0, array, arrayIndex, Count);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Enumerate(_root);
        }

        public int IndexOf(T item)
        {
            var comparer = EqualityComparer<T>.Default;
            Int32 idx = 0;
            foreach (var elem in this)
            {
                if (comparer.Equals(elem, item))
                {
                    return idx;
                }
                ++idx;
            }
            return -1;
        }

        public void Insert(int index, T item)
        {
            InsertRange(index, new T[] { item, });
        }

        public bool Remove(T item)
        {
            Int32 idx = IndexOf(item);
            if (idx >= 0)
            {
                RemoveAt(idx);
                return true;
            }
            return false;
        }

        public void RemoveAt(int index)
        {
            RemoveRange(index, 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            var ret = new Rope<T>();
            ret._root = _root.Clone();
            return ret;
        }
        #endregion

        #region Other operations
        public void AddRange(T[] items)
        {
            InsertRange(_root.Length, items);
#if DEBUG
            VerifySelf();
#endif
        }

        public void InsertRange(Int32 index, T[] items)
        {
            InnerInsert(_root, index, items, 0, items.Length);
#if DEBUG
            VerifySelf();
#endif
        }

        public void RemoveRange(Int32 index, Int32 length)
        {
            VerifyRange(index, length);
            InnerRemove(_root, index, length);
#if DEBUG
            VerifySelf();
#endif
        }

        public void Replace(Int32 index, Int32 length, T[] items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            VerifyRange(index, length);
            if (length == 0 && items.Length == 0)
            {
                return;
            }
            if (length == 0 && items.Length > 0)
            {
                InsertRange(index, items);
            }
            else if (length > 0 && items.Length == 0)
            {
                RemoveRange(index, length);
            }
            else
            {
                InnerReplace(_root, index, items, 0, items.Length);
            }
#if DEBUG
            VerifySelf();
#endif
        }

        public void CopyTo(Int32 index, T[] array, Int32 arrayIndex, Int32 length)
        {
            VerifyRange(index, length);
            InnerCopyTo(_root, index, array, arrayIndex, length);
        }
        #endregion

        #region Private
        internal void VerifyRange(Int32 startIndex, Int32 length)
        {
            if (startIndex < 0 || startIndex >= Count)
            {
                throw new ArgumentOutOfRangeException("startIndex", startIndex, "0 <= startIndex <= " + Count.ToString(CultureInfo.InvariantCulture));
            }
            if (length < 0 || startIndex + length > Count)
            {
                throw new ArgumentOutOfRangeException("length", length, "0 <= length, startIndex(" + startIndex + ") + length <= " + Count.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal void VerifyNodeRange(RopeNode node, Int32 startIndex, Int32 length)
        {
            if (startIndex < 0 || startIndex >= node.Length)
            {
                throw new ArgumentOutOfRangeException("startIndex", startIndex, "0 <= startIndex <= " + node.Length.ToString(CultureInfo.InvariantCulture));
            }
            if (length < 0 || startIndex + length > node.Length)
            {
                throw new ArgumentOutOfRangeException("length", length, "0 <= length, startIndex(" + startIndex + ") + length <= " + node.Length.ToString(CultureInfo.InvariantCulture));
            }
        }

        internal void InnerCopyTo(RopeNode node, Int32 offset, T[] array, Int32 arrayIndex, Int32 length)
        {
            if (length <= 0)
            {
                return;
            }
            if (node.IsLeaf)
            {
                node.GenerateContentsIfRequired();
                Array.Copy(node._contents, offset, array, arrayIndex, length);
            }
            else
            {
                if (offset < node.Left.Length)
                {
                    Int32 leftSize = Math.Min(length, node.Left.Length - offset);
                    InnerCopyTo(node.Left, offset, array, arrayIndex, leftSize);
                    if (node.Left.Length < offset + length)
                    {
                        Int32 rightSize = offset + length - node.Left.Length;
                        InnerCopyTo(node.Right, 0, array, arrayIndex + leftSize, rightSize);
                    }
                }
                else
                {
                    offset -= node.Left.Length;
                    InnerCopyTo(node.Right, offset, array, arrayIndex, length);
                }
            }
        }

        internal void InnerInsert(RopeNode node, Int32 offset, T[] array, Int32 arrayIndex, Int32 length)
        {
            if (length <= 0)
            {
                return;
            }
            if (node.Length + length < RopeNode.NODE_SIZE)
            {
                // 一定为叶子节点
                node.GenerateContentsIfRequired();
                // 先数据后移
                for (int i = node.Length - 1; i >= offset; i--)
                {
                    node._contents[i + length] = node._contents[i];
                }
                // 再拷贝数据
                Array.Copy(array, arrayIndex, node._contents, offset, length);
                node.Length += length;
            }
            else if (node.IsLeaf)
            {
                // 叶子节点，且当前叶子节点存储不下的情况
                node.GenerateContentsIfRequired();
                /* 假设当前节点为A，创建新节点B，并创建新节点C，将B和A分别作为C的左右孩子，C节点替代A节点的位置
                 *       A                     C
                 *                ===>        / \
                 *                           B   A
                 */
                var a = node;
                var b = new RopeNode();
                b.GenerateContentsIfRequired();
                var c = new RopeNode();
                if (a.Parent != null)
                {
                    if (a.IsLeft)
                    {
                        a.Parent.Left = c;
                    }
                    else
                    {
                        a.Parent.Right = c;
                    }
                }
                else
                {
                    _root = c;
                }
                c.Left = b;
                b.Parent = c;
                c.Right = a;
                a.Parent = c;
                // 将[0, offset)拷贝到左节点
                Int32 distributeLeft = offset;
                if (distributeLeft > 0)
                {
                    Array.Copy(a._contents, 0, b._contents, 0, distributeLeft);
                    // 将a后面的节点往前移动
                    for (int i = distributeLeft; i < a.Length; i++)
                    {
                        a._contents[i - distributeLeft] = a._contents[i];
                    }
                    a.Length -= distributeLeft;
                }
                Int32 rest = Math.Min(length, RopeNode.NODE_SIZE - distributeLeft);
                Array.Copy(array, arrayIndex, b._contents, distributeLeft, rest);
                b.Length = distributeLeft + rest;
                arrayIndex += rest;
                length -= rest;
                offset = 0;
                InnerInsert(node, offset, array, arrayIndex, length);
                // 自平衡
                Rebalance(c);
            }
            else
            {
                node.Length += length;
                // 非叶子节点不存储数据，只用来二分
                if (offset < node.Left.Length)
                {
                    InnerInsert(node.Left, offset, array, arrayIndex, length);
                }
                else
                {
                    offset -= node.Left.Length;
                    InnerInsert(node.Right, offset, array, arrayIndex, length);
                }
                // 只有非叶子节点才需要重新平衡
                Rebalance(node);
            }
        }

        internal void InnerRemove(RopeNode node, Int32 offset, Int32 length)
        {
            if (length <= 0)
            {
                return;
            }
            if (node.IsLeaf)
            {
                // 叶子节点，直接数据前移
                for (int i = offset + length; i < node.Length; i++)
                {
                    node._contents[i - length] = node._contents[i];
                }
                node.Length -= length;
            }
            else
            {
                node.Length -= length;
                Int32 leftLength = node.Left.Length;
                if (offset < leftLength)
                {
                    Int32 leftSize = Math.Min(length, leftLength - offset);
                    InnerRemove(node.Left, offset, leftSize);
                    if (leftLength < offset + length)
                    {
                        Int32 rightSize = offset + length - leftLength;
                        InnerRemove(node.Right, 0, rightSize);
                    }
                }
                else
                {
                    offset -= leftLength;
                    InnerRemove(node.Right, offset, length);
                }
                //  合并节点
                RopeNode mergedNode = Merge(node);
                // 自平衡
                Rebalance(mergedNode);
            }
        }

        internal void InnerReplace(RopeNode node, Int32 offset, T[] array, Int32 arrayIndex, Int32 length)
        {
            if (length <= 0)
            {
                return;
            }
            if (node.IsLeaf)
            {
                node.GenerateContentsIfRequired();
                Array.Copy(array, arrayIndex, node._contents, offset, length);
            }
            else
            {
                if (offset < node.Left.Length)
                {
                    Int32 leftSize = Math.Min(length, node.Left.Length - offset);
                    InnerReplace(node.Left, offset, array, arrayIndex, leftSize);
                    if (node.Left.Length < offset + length)
                    {
                        Int32 rightSize = offset + length - node.Left.Length;
                        InnerReplace(node.Right, 0, array, arrayIndex + leftSize, rightSize);
                    }
                }
                else
                {
                    offset -= node.Left.Length;
                    InnerReplace(node.Right, offset, array, arrayIndex, length);
                }
            }
        }

        internal T InnerGetElement(RopeNode node, Int32 offset)
        {
            VerifyNodeRange(node, offset, 1);
            if (node.IsLeaf)
            {
                return node._contents[offset];
            }
            if (offset < node.Left.Length)
            {
                return InnerGetElement(node.Left, offset);
            }
            offset -= node.Left.Length;
            return InnerGetElement(node.Right, offset);
        }

        internal void InnerSetElement(RopeNode node, Int32 offset, T elem)
        {
            VerifyNodeRange(node, offset, 1);
            if (node.IsLeaf)
            {
                node._contents[offset] = elem;
            }
            else if (offset < node.Left.Length)
            {
                InnerSetElement(node.Left, offset, elem);
            }
            else
            {
                offset -= node.Left.Length;
                InnerSetElement(node.Right, offset, elem);
            }
        }

        internal IEnumerable<RopeNode> Leaves(RopeNode node)
        {
            var ret = new List<RopeNode>();
            var stack = new Stack<RopeNode>();
            while (node != null)
            {
                while (!node.IsLeaf)
                {
                    stack.Push(node.Right);
                    node = node.Left;
                }
                ret.Add(node);
                if (stack.Count > 0)
                {
                    node = stack.Pop();
                }
                else
                {
                    node = null;
                }
            }
            return ret;
        }

        internal RopeNode Merge(RopeNode node)
        {
            if (node.IsLeaf)
            {
                return node;
            }
            // 如果左右孩子长度都不为0，则分两种情况
            if (node.Left.Length > 0 && node.Right.Length > 0)
            {
                // 长度和不大于NODE_SIZE，则将所有数据合并到最左侧节点，并用最左侧节点替换原节点
                if (node.Left.Length + node.Right.Length <= RopeNode.NODE_SIZE)
                {
                    var stack = new Stack<RopeNode>();
                    var curNode = node;
                    RopeNode leftMost = null;
                    Int32 offset = 0;
                    while (curNode != null)
                    {
                        while (!curNode.IsLeaf)
                        {
                            stack.Push(curNode.Right);
                            curNode = curNode.Left;
                        }
                        if (leftMost == null)
                        {
                            leftMost = curNode;
                            offset = curNode.Length;
                        }
                        else
                        {
                            Array.Copy(curNode._contents, 0, leftMost._contents, offset, curNode.Length);
                            offset += curNode.Length;
                        }
                    }
                    Debug.Assert(leftMost != null, "LeftMost couldn't be null.");
                    leftMost.Length = node.Left.Length + node.Right.Length;
                    if (node.IsLeft)
                    {
                        node.Parent.Left = leftMost;
                        leftMost.Parent = node.Parent;
                    }
                    else if (node.IsRight)
                    {
                        node.Parent.Right = leftMost;
                        leftMost.Parent = node.Parent;
                    }
                    else
                    {
                        _root = leftMost;
                        leftMost.Parent = null;
                    }
                    return leftMost;
                }
                // 如果大于NODE_SIZE则不做处理
            }
            else if (node.Left.Length > 0)
            {
                // 左孩子长度大于0，则用左孩子代替原节点
                if (node.IsLeft)
                {
                    node.Parent.Left = node.Left;
                    node.Left.Parent = node.Parent;
                }
                else if (node.IsRight)
                {
                    node.Parent.Right = node.Left;
                    node.Left.Parent = node.Parent;
                }
                else
                {
                    _root = node.Left;
                    node.Left.Parent = null;
                }
                return node.Left;
            }
            else
            {
                // 右孩子长度大于0或两个孩子都为空，则用右孩子代替原节点
                if (node.IsLeft)
                {
                    node.Parent.Left = node.Right;
                    node.Right.Parent = node.Parent;
                }
                else if (node.IsRight)
                {
                    node.Parent.Right = node.Right;
                    node.Right.Parent = node.Parent;
                }
                else
                {
                    _root = node.Right;
                    node.Right.Parent = null;
                }
                return node.Right;
            }
            return node;
        }

        internal void Rebalance(RopeNode node)
        {
            if (node.IsLeaf)
            {
                return;
            }
            while (Math.Abs(node.Balance) > 1)
            {
                if (node.Balance > 1)
                {
                    if (node.Left.Balance < 0)
                    {
                        RotateLeft(node.Left);
                    }
                    RotateRight(node);
                    Rebalance(node.Right);
                }
                else if (node.Balance < -1)
                {
                    if (node.Right.Balance > 0)
                    {
                        RotateRight(node.Right);
                    }
                    RotateLeft(node);
                    Rebalance(node.Left);
                }
            }
            node.Height = (Byte)(1 + Math.Max(node.Left.Height, node.Right.Height));
            node.Length = node.Left.Length + node.Right.Length;
        }

        internal void RotateLeft(RopeNode node)
        {
            Debug.Assert(!node.IsLeaf);
            /* 和二叉搜索树的左旋不同，它只需要保证叶子节点的顺序一致
			 * 
			 *       node               node
			 *       /  \               /  \
			 *      A   right  ===>  right  C
			 *           / \          / \
			 *          B   C        A   B
			 */
            var right = node.Right;
            var a = node.Left;
            var b = right.Left;
            var c = right.Right;
            node.Left = right;
            right.Parent = node;
            right.Left = a;
            a.Parent = right;
            right.Right = b;
            if (b != null)
            {
                b.Parent = right;
            }
            node.Right = c;
            if (c != null)
            {
                c.Parent = node;
            }
            right.Height = (Byte)(1 + Math.Max(right.Left.Height, right.Right.Height));
            right.Length = right.Left.Length + right.Right.Length;
        }

        internal void RotateRight(RopeNode node)
        {
            Debug.Assert(!node.IsLeaf);
            /* 和二叉搜索树的右旋不同，它只需要保证叶子节点的顺序一致
			 * 
			 *       node             node
			 *       /  \             /  \
			 *     left  C   ===>    A   left
			 *     / \                   /  \
			 *    A   B                 B    C
			 */
            var left = node.Left;
            var a = left.Left;
            var b = left.Right;
            var c = node.Right;
            node.Left = a;
            if (a != null)
            {
                a.Parent = node;
            }
            node.Right = left;
            left.Parent = node;
            left.Left = b;
            if (b != null)
            {
                b.Parent = left;
            }
            left.Right = c;
            c.Parent = left;
            left.Height = (Byte)(1 + Math.Max(left.Left.Height, left.Right.Height));
            left.Length = left.Left.Length + left.Right.Length;
        }
        #endregion

        #region Enumeration
        private IEnumerator<T> Enumerate(RopeNode node)
        {
            var stack = new Stack<RopeNode>();
            while (node != null)
            {
                while (!node.IsLeaf)
                {
                    stack.Push(node.Right);
                    node = node.Left;
                }
                // yield return leaf node contents
                for (int i = 0; i < node.Length; i++)
                {
                    yield return node._contents[i];
                }
                if (stack.Count > 0)
                {
                    node = stack.Pop();
                }
                else
                {
                    node = null;
                }
            }
        }
        #endregion

        #region Debug
        [Conditional("DEBUG")]
        internal void VerifySelf()
        {
            VerifyNode(_root);
        }
        [Conditional("DEBUG")]
        internal void VerifyNode(RopeNode node)
        {
            if (node == null)
            {
                return;
            }
            Debug.Assert(Math.Abs(node.Balance) <= 1);
            Debug.Assert(node.IsLeaf || node.Length == node.Left.Length + node.Right.Length);
            VerifyNode(node.Left);
            VerifyNode(node.Right);
        }
        #endregion

        internal RopeNode _root;
    }
}
