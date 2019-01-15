using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UltraEmeraldScriptEditor.DataStructure
{
    /// <summary>
    /// AVL树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AvlTree<T> : ICollection<T>
        where T : IComparable
    {
        private class TreeNode : ITreeNode<T>
        {
            public Boolean IsLeftChild
            {
                get
                {
                    return Parent != null && Parent.LeftChild == this;
                }
            }
            public Boolean IsRightChild
            {
                get
                {
                    return Parent != null && Parent.RightChild == this;
                }
            }
            public T Data { get; set; }
            public ITreeNode<T> Parent { get; set; }
            public ITreeNode<T> LeftChild { get; set; }
            public ITreeNode<T> RightChild { get; set; }
            public Int32 Balance { get; set; }

            public override string ToString()
            {
                return Data.ToString() + "\tBalance = " + Balance.ToString();
            }
        }

        #region CONSTRUCTOR
        public AvlTree(Boolean allowDuplication = false)
        {
            _size = 0;
            _allowDuplication = allowDuplication;
        }
        public AvlTree(IEnumerable<T> elements, Boolean allowDuplication = false)
            : this(allowDuplication)
        {
            foreach (var elem in elements)
            {
                Add(elem);
            }
        }
        #endregion

        #region ICollection<T>
        public int Count
        {
            get { return _size; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            var newNode = new TreeNode
            {
                Data = item,
                Balance = 0,
            };
            if (_root == null)
            {
                _root = newNode;
            }
            else
            {
                TreeNode curNode = _root;
                while (true)
                {
                    if (!_allowDuplication && item.CompareTo(curNode.Data) == 0)
                    {
                        return;
                    }
                    Boolean precedes = item.CompareTo(curNode.Data) < 0;
                    if (precedes)
                    {
                        if (curNode.LeftChild != null)
                        {
                            curNode = curNode.LeftChild as TreeNode;
                        }
                        else
                        {
                            curNode.LeftChild = newNode;
                            newNode.Parent = curNode;
                            InsertBalance(curNode, 1);
                            break;
                        }
                    }
                    else
                    {
                        if (curNode.RightChild != null)
                        {
                            curNode = curNode.RightChild as TreeNode;
                        }
                        else
                        {
                            curNode.RightChild = newNode;
                            newNode.Parent = curNode;
                            InsertBalance(curNode, -1);
                            break;
                        }
                    }
                }
            }
            ++_size;
        }

        public void Clear()
        {
            _root = null;
            _size = 0;
        }

        public bool Contains(T item)
        {
            if (_root == null)
            {
                return false;
            }

            ITreeNode<T> curNode = _root;
            while (true)
            {
                if (item.CompareTo(curNode.Data) == 0)
                {
                    return true;
                }
                Boolean precedes = item.CompareTo(curNode.Data) < 0;
                if (precedes)
                {
                    if (curNode.LeftChild != null)
                    {
                        curNode = curNode.LeftChild;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (curNode.RightChild != null)
                    {
                        curNode = curNode.RightChild;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Int32 i = 0;
            foreach (var item in this)
            {
                array[i + arrayIndex] = item;
                ++i;
            }
        }

        public bool Remove(T item)
        {
            TreeNode curNode = _root;
            while (curNode != null)
            {
                if (item.CompareTo(curNode.Data) == 0)
                {
                    RemoveNode(curNode);
                    return true;
                }
                else if (item.CompareTo(curNode.Data) < 0)
                {
                    curNode = curNode.LeftChild as TreeNode;
                }
                else
                {
                    curNode = curNode.RightChild as TreeNode;
                }
            }
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new BinaryTreeEnumerator<T>(_root);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region PRIVATE
        private ITreeNode<T> FindPredecessor(ITreeNode<T> node)
        {
            // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
            if (node.LeftChild != null)
            {
                node = node.LeftChild;
                while (node.RightChild != null)
                {
                    node = node.RightChild;
                }
                return node;
            }
            // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
            while (node.Parent != null && node.Parent.LeftChild == node)
            {
                node = node.Parent;
            }
            return node.Parent;
        }

        private ITreeNode<T> FindSuccessor(ITreeNode<T> node)
        {
            // 存在右子树，则后继节点就是右子树最靠左的叶子节点
            if (node.RightChild != null)
            {
                node = node.RightChild;
                while (node.LeftChild != null)
                {
                    node = node.LeftChild;
                }
                return node;
            }
            // 不存在右子树，后继节点则为向上第一个右子树中不能存在该节点的节点
            while (node.Parent != null && node.Parent.RightChild == node)
            {
                node = node.Parent;
            }
            return node.Parent;
        }

        private void RemoveNode(TreeNode node)
        {
            // 平衡相关
            TreeNode start = null;
            Int32 balance = 0;
            // 替换node的节点
            TreeNode toReplace = null;
            if (node.LeftChild == null && node.RightChild == null)
            {
                // 直接删除
                toReplace = null;
                if (node.Parent == null)
                {
                    _root = toReplace;
                }
                else if (node.IsLeftChild)
                {
                    node.Parent.LeftChild = toReplace;

                    start = node.Parent as TreeNode;
                    balance = -1;
                }
                else
                {
                    node.Parent.RightChild = toReplace;

                    start = node.Parent as TreeNode;
                    balance = 1;
                }
            }
            else if (node.LeftChild == null || node.RightChild == null)
            {
                // 用唯一的子节点替换该节点
                toReplace = node.LeftChild != null ? (node.LeftChild as TreeNode) : (node.RightChild as TreeNode);
                if (node.Parent == null)
                {
                    _root = toReplace;

                    start = toReplace;
                    balance = toReplace.IsLeftChild ? -1 : 1;
                }
                else if (node.IsLeftChild)
                {
                    node.Parent.LeftChild = toReplace;

                    start = node.Parent as TreeNode;
                    balance = -1;
                }
                else
                {
                    node.Parent.RightChild = toReplace;

                    start = node.Parent as TreeNode;
                    balance = 1;
                }
                toReplace.Parent = node.Parent;
            }
            else
            {
                // 用后继节点替换该节点
                // 因为有右子树所以successor不可能为空
                toReplace = FindSuccessor(node) as TreeNode;
                // 该后继节点只可能有右孩子，所以将右孩子和父节点连接。
                if (toReplace.IsLeftChild)
                {
                    toReplace.Parent.LeftChild = toReplace.RightChild;
                    balance = -1;
                }
                else
                {
                    toReplace.Parent.RightChild = toReplace.RightChild;
                    balance = 1;
                }
                if (toReplace.RightChild != null)
                {
                    toReplace.RightChild.Parent = toReplace.Parent;
                }
                // 情况过于复杂，所以只替换数据
                node.Data = toReplace.Data;

                start = toReplace.Parent as TreeNode;
            }
            --_size;

            // 处理树的平衡
            if (start != null)
            {
                RemoveBalance(start, balance);
            }
        }

        private void InsertBalance(TreeNode node, Int32 balance)
        {
            while (node != null)
            {
                node.Balance += balance;
                if (node.Balance == 0)
                {
                    // 平衡因子不变化，树已经平衡
                    return;
                }

                if (node.Balance == 2)
                {
                    // 此时左孩子平衡因子非1即-1
                    if ((node.LeftChild as TreeNode).Balance == 1)
                    {
                        RotateRight(node);
                    }
                    else
                    {
                        RotateLeftRight(node);
                    }

                    return;
                }
                else if (node.Balance == -2)
                {
                    // 此时右孩子平衡因子非-1即1
                    if ((node.RightChild as TreeNode).Balance == -1)
                    {
                        RotateLeft(node);
                    }
                    else
                    {
                        RotateRightLeft(node);
                    }

                    return;
                }

                if (node.IsLeftChild)
                {
                    balance = 1;
                }
                else if (node.IsRightChild)
                {
                    balance = -1;
                }
                node = node.Parent as TreeNode;
            }
        }

        private void RemoveBalance(TreeNode node, Int32 balance)
        {
            while (node != null)
            {
                node.Balance += balance;

                if (node.Balance == 2)
                {
                    // 此时左孩子平衡因子非1即-1
                    if ((node.LeftChild as TreeNode).Balance == 1)
                    {
                        node = RotateRight(node);
                    }
                    else
                    {
                        node = RotateLeftRight(node);
                    }
                }
                else if (node.Balance == -2)
                {
                    // 此时右孩子平衡因子非-1即1
                    if ((node.RightChild as TreeNode).Balance == -1)
                    {
                        node = RotateLeft(node);
                    }
                    else
                    {
                        node = RotateRightLeft(node);
                    }
                }

                if (node.IsLeftChild)
                {
                    balance = -1;
                }
                else if (node.IsRightChild)
                {
                    balance = 1;
                }
                node = node.Parent as TreeNode;
            }
        }

        private TreeNode RotateLeft(TreeNode node)
        {
            TreeNode right = node.RightChild as TreeNode;
            TreeNode rightLeft = right.LeftChild as TreeNode;
            TreeNode parent = node.Parent as TreeNode;

            right.LeftChild = node;
            node.Parent = right;
            node.RightChild = rightLeft;
            if (rightLeft != null)
            {
                rightLeft.Parent = node;
            }
            right.Parent = parent;
            if (parent == null)
            {
                _root = right as TreeNode;
            }
            else if (parent.LeftChild == node)
            {
                parent.LeftChild = right;
            }
            else
            {
                parent.RightChild = right;
            }

            ++right.Balance;
            node.Balance = -right.Balance;

            return right;
        }

        private TreeNode RotateRight(TreeNode node)
        {
            TreeNode left = node.LeftChild as TreeNode;
            TreeNode leftRight = left.RightChild as TreeNode;
            TreeNode parent = node.Parent as TreeNode;

            left.RightChild = node;
            node.Parent = left;
            node.LeftChild = leftRight;
            if (leftRight != null)
            {
                leftRight.Parent = node;
            }
            left.Parent = parent;
            if (parent == null)
            {
                _root = left as TreeNode;
            }
            else if (parent.LeftChild == node)
            {
                parent.LeftChild = left;
            }
            else
            {
                parent.RightChild = left;
            }

            --left.Balance;
            node.Balance = -left.Balance;

            return left;
        }

        private TreeNode RotateLeftRight(TreeNode node)
        {
            TreeNode left = node.LeftChild as TreeNode;
            TreeNode leftRight = left.RightChild as TreeNode;
            TreeNode parent = node.Parent as TreeNode;
            TreeNode leftRightLeft = leftRight.LeftChild as TreeNode;
            TreeNode leftRightRight = leftRight.RightChild as TreeNode;
            // 先左旋
            left.Parent = leftRight;
            leftRight.LeftChild = left;
            leftRight.Parent = node;
            node.LeftChild = leftRight;
            left.RightChild = leftRightLeft;
            if (leftRightLeft != null)
            {
                leftRightLeft.Parent = left;
            }
            // 再右旋node
            leftRight.RightChild = node;
            node.Parent = leftRight;
            node.LeftChild = leftRightRight;
            if (leftRightRight != null)
            {
                leftRightRight.Parent = node;
            }
            leftRight.Parent = parent;
            if (parent == null)
            {
                _root = leftRight;
            }
            else if (parent.LeftChild == node)
            {
                parent.LeftChild = leftRight;
            }
            else
            {
                parent.RightChild = leftRight;
            }
            if (leftRight.Balance == 0)
            {
                node.Balance = 0;
                left.Balance = 0;
            }
            else if (leftRight.Balance == 1)
            {
                node.Balance = -1;
                left.Balance = 0;
            }
            else
            {
                node.Balance = 0;
                left.Balance = 1;
            }
            leftRight.Balance = 0;

            return leftRight;
        }

        private TreeNode RotateRightLeft(TreeNode node)
        {
            TreeNode right = node.RightChild as TreeNode;
            TreeNode rightLeft = right.LeftChild as TreeNode;
            TreeNode parent = node.Parent as TreeNode;
            TreeNode rightLeftLeft = rightLeft.LeftChild as TreeNode;
            TreeNode rightLeftRight = rightLeft.RightChild as TreeNode;
            // 先右旋
            right.Parent = rightLeft;
            rightLeft.RightChild = right;
            rightLeft.Parent = node;
            node.RightChild = rightLeft;
            right.LeftChild = rightLeftRight;
            if (rightLeftRight != null)
            {
                rightLeftRight.Parent = right;
            }
            // 再左旋
            rightLeft.LeftChild = node;
            node.Parent = rightLeft;
            node.RightChild = rightLeftLeft;
            if (rightLeftLeft != null)
            {
                rightLeftLeft.Parent = node;
            }
            rightLeft.Parent = parent;
            if (parent == null)
            {
                _root = rightLeft;
            }
            else if (parent.LeftChild == node)
            {
                parent.LeftChild = rightLeft;
            }
            else
            {
                parent.RightChild = rightLeft;
            }
            if (rightLeft.Balance == 0)
            {
                node.Balance = 0;
                right.Balance = 0;
            }
            else if (rightLeft.Balance == 1)
            {
                node.Balance = 0;
                right.Balance = -1;
            }
            else
            {
                node.Balance = 1;
                right.Balance = 0;
            }
            rightLeft.Balance = 0;

            return rightLeft;
        }

        private TreeNode _root;
        private Int32 _size;
        private Boolean _allowDuplication;
        #endregion

        #region FOR DEBUG
        [Conditional("DEBUG")]
        /// <summary>
        /// 层次遍历
        /// </summary>
        /// <param name="action"></param>
        public void LevelTraversal(Action<ITreeNode<T>> action)
        {
            if (_root == null || action == null)
            {
                return;
            }
            // FIFO特性
            var nodeQueue = new Queue<ITreeNode<T>>();
            nodeQueue.Enqueue(_root);
            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                action(curNode);
                if (curNode.LeftChild != null)
                {
                    nodeQueue.Enqueue(curNode.LeftChild);
                }
                if (curNode.RightChild != null)
                {
                    nodeQueue.Enqueue(curNode.RightChild);
                }
            }
        }

        [Conditional("DEBUG")]
        public void CheckValid()
        {
            Int32 size = _size;
            Int32 chkSize = 0;
            LevelTraversal(node =>
            {
                TreeNode avlNode = node as TreeNode;
                if (avlNode.LeftChild != null && avlNode.LeftChild.Data.CompareTo(avlNode.Data) > 0)
                {
                    throw new Exception("Invalid BSTree");
                }
                else if (avlNode.RightChild != null && avlNode.RightChild.Data.CompareTo(avlNode.Data) < 0)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (avlNode.Parent == null && avlNode != _root)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (avlNode.Parent != null && avlNode.Parent.LeftChild != avlNode && avlNode.Parent.RightChild != avlNode)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (avlNode.Balance < -1 || avlNode.Balance > 1)
                {
                    throw new Exception("Invalid BSTree");
                }
                ++chkSize;
            });
            if (chkSize != size)
            {
                throw new Exception("Invalid BSTree");
            }
        }
        public String Draw()
        {
            var sb = new StringBuilder();
            Int32 lastLine = 0;
            LevelTraversal(node =>
            {
                Int32 line = 0;
                var cur = node;
                while (cur != null)
                {
                    ++line;
                    cur = cur.Parent;
                }
                if (line != lastLine)
                {
                    sb.AppendLine();
                    lastLine = line;
                }
                sb.Append(node.Data).Append(" ");
            });
            return sb.ToString();
        }
        #endregion
    }
}
