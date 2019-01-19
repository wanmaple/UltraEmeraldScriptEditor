using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 由于需要快速插入/删除字符，List的插入和删除性能是满足不了的
    /// 我们用一颗自平衡的二叉树来解决这个问题
    /// 我们将数据只存储到叶子节点中，每个节点记录自己子树的总长度
    /// 靠左的叶子节点的起始偏移小于靠右的叶子节点的起始偏移
    /// 优点在于寻找偏移的时候效率为O(log N)，删除数据的时候不需要做数据移动
    /// 插入操作略微复杂一些，当插入数据的时候，需要将子节点与新的子树重新设置一个父节点，然后自平衡，在插入的数据量比较大时，会经过多次自平衡
    /// 而且插入数据量比较大的时候，还有O(N)的复制消耗，插入操作也许还有优化的空间？？？
    /// 综上所述，对于频繁插入/删除字符，且数据量很大的时候会有比较客观的性能
    /// 缺点：查找性能欠缺
    /// </summary>
    /// <remarks>
    /// 线程不安全
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public sealed class Rope<T> : IList<T>, ICloneable
    {
        /// <summary>
        /// RopeNode只有叶子结点才负责存储数据，任何高度>0的节点只是负责存储所有子树的总长度
        /// 从左到右依次列出的叶子节点的数据连起来就是根节点所表示的连续数据
        /// </summary>
        internal sealed class RopeNode
        {
            internal static readonly Int32 NODE_SIZE = 2;

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
        } 
        #endregion

        #region IList<T>
        public T this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ICloneable
        public object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Other extension operations
        public void AddRange(T[] items)
        {
            InsertRange(_root.Length, items);
        }

        public void InsertRange(Int32 index, T[] items)
        {
            InnerInsert(_root, index, items, 0, items.Length);
        }

        public void RemoveRange(Int32 index, Int32 length)
        {

        }
        #endregion

        #region Private
        private void InnerInsert(RopeNode node, Int32 offset, T[] array, Int32 arrayIndex, Int32 count)
        {
            if (count <= 0)
            {
                return;
            }
            if (node.Length + count < RopeNode.NODE_SIZE)
            {
                // 一定为叶子节点
                node.GenerateContentsIfRequired();
                // 先数据后移
                for (int i = node.Length - 1; i >= offset; i--)
                {
                    node._contents[i + count] = node._contents[i];
                }
                // 再拷贝数据
                Array.Copy(array, arrayIndex, node._contents, offset, count);
                node.Length += count;
            }
            else if (node.Height == 0)
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
                Int32 rest = Math.Min(count, RopeNode.NODE_SIZE - distributeLeft);
                Array.Copy(array, arrayIndex, b._contents, distributeLeft, rest);
                b.Length = distributeLeft + rest;
                arrayIndex += rest;
                count -= rest;
                offset = 0;
                InnerInsert(node, offset, array, arrayIndex, count);
                // 自平衡
                Rebalance(c);
            }
            else
            {
                node.Length += count;
                // 非叶子节点不存储数据，只用来二分
                if (offset < node.Left.Length)
                {
                    InnerInsert(node.Left, offset, array, arrayIndex, count);
                }
                else
                {
                    offset -= node.Left.Length;
                    InnerInsert(node.Right, offset, array, arrayIndex, count);
                }
                // 只有非叶子节点才需要重新平衡
                Rebalance(node);
            }
        }

        private void Rebalance(RopeNode node)
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

        private void RotateLeft(RopeNode node)
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

        private void RotateRight(RopeNode node)
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

        private RopeNode _root;
    }
}
