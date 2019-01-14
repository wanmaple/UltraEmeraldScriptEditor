using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UltraEmeraldScriptEditor.DataStructure
{
    /// <summary>
    /// 红黑树
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RedBlackTree<T> : ICollection<T>
        where T : IComparable
    {
        private enum NodeColor
        {
            RED = 0,
            BLACK = 1,
            DOUBLE_BLACK = 2,
        }
        private class TreeNode : ITreeNode<T>
        {
            public static TreeNode DoubleBlackNilNode => new TreeNode { Color = NodeColor.DOUBLE_BLACK, };
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
            public NodeColor Color { get; set; }
        }

        #region CONSTRUCTOR
        public RedBlackTree(Boolean allowDuplication = false)
        {
            _size = 0;
            _allowDuplication = allowDuplication;
        }
        public RedBlackTree(IEnumerable<T> elements, Boolean allowDuplication = false)
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
                Color = NodeColor.RED,
            };
            if (_root == null)
            {
                _root = newNode;
                // 根节点只能是黑色
                newNode.Color = NodeColor.BLACK;
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
                            FixTree4Insertion(newNode);
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
                            FixTree4Insertion(newNode);
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
        private void FixTree4Insertion(TreeNode node)
        {
            TreeNode parent = node.Parent as TreeNode;
            if (parent == null)
            {
                // 根节点只能是黑色
                node.Color = NodeColor.BLACK;
                return;
            }
            TreeNode grandParent = parent.Parent as TreeNode;
            if (grandParent == null)
            {
                return;
            }
            TreeNode uncle = null;
            if (parent.IsLeftChild)
            {
                uncle = grandParent.RightChild as TreeNode;
            }
            else
            {
                uncle = grandParent.LeftChild as TreeNode;
            }
            // NIL节点都是黑色的
            NodeColor uncleColor = uncle == null ? NodeColor.BLACK : uncle.Color;
            // 只有parent是红色才需要做着色或旋转操作
            if (parent.Color == NodeColor.RED)
            {
                // 如果uncle是红色，我们只需要重新着色
                if (uncleColor == NodeColor.RED)
                {
                    // 将parent和uncle节点置为黑色
                    parent.Color = NodeColor.BLACK;
                    uncle.Color = NodeColor.BLACK;
                    // 将grandparent节点置为红色
                    grandParent.Color = NodeColor.RED;
                    FixTree4Insertion(grandParent);
                }
                else
                {
                    // 如果uncle是黑色，分为四种情况
                    // 1. node是左孩子，parent是左孩子
                    if (node.IsLeftChild && parent.IsLeftChild)
                    {
                        // 右旋
                        RotateRight(grandParent);
                        // 将grandparent和parent换色
                        parent.Color = NodeColor.BLACK;
                        grandParent.Color = NodeColor.RED;
                    }
                    // 2. node是右孩子，parent是左孩子
                    else if (node.IsRightChild && parent.IsLeftChild)
                    {
                        // 左右旋
                        RotateLeftRight(grandParent);
                        // 将node和grandparent换色
                        node.Color = NodeColor.BLACK;
                        grandParent.Color = NodeColor.RED;
                    }
                    // 3. node是右孩子，parent是右孩子
                    else if (node.IsRightChild && parent.IsRightChild)
                    {
                        // 左旋
                        RotateLeft(grandParent);
                        // 将grandparent和parent换色
                        parent.Color = NodeColor.BLACK;
                        grandParent.Color = NodeColor.RED;
                    }
                    // 4. node是左孩子，parent是右孩子
                    else
                    {
                        // 右左旋
                        RotateRightLeft(grandParent);
                        // 将node和grandparent换色
                        node.Color = NodeColor.BLACK;
                        grandParent.Color = NodeColor.RED;
                    }
                    // 根节点只能是黑色
                    _root.Color = NodeColor.BLACK;
                }
            }
        }

        private void FixTree4Deletion(TreeNode node, Boolean remove = true)
        {
            // 只有Double-Black的节点需要修复，其他情况已经在NodeToReplace中正确处理
            if (node.Color != NodeColor.DOUBLE_BLACK)
            {
                return;
            }
            TreeNode parent = node.Parent as TreeNode;
            TreeNode sibling = node.IsLeftChild ? (parent.RightChild as TreeNode) : (parent.LeftChild as TreeNode);
            if (node.IsLeftChild)
            {
                if (parent.Color == NodeColor.BLACK && sibling.Color == NodeColor.BLACK)
                {
                    PullBlack(parent);
                    // 如果sibling有子节点，那么子节点一定是红色
                    if (sibling.LeftChild != null && sibling.RightChild != null)
                    {
                        RotateLeft(parent);
                        SwapColors(parent, sibling);
                        PullBlack(sibling);
                    }
                    else if (sibling.LeftChild == null && sibling.RightChild == null)
                    {
                        FixTree4Deletion(parent, false);
                    }
                    else if (sibling.LeftChild != null)
                    {
                        // 和插入操作相同
                        TreeNode siblingLeft = sibling.LeftChild as TreeNode;
                        RotateRightLeft(parent);
                        SwapColors(siblingLeft, parent);
                        // 将Double-Black push到两个红色节点即可达成修复
                        PushBlack(siblingLeft);
                    }
                    else
                    {
                        // 和插入操作相同
                        RotateLeft(parent);
                        SwapColors(parent, sibling);
                        // 将Double-Black push到两个红色节点即可达成修复
                        PushBlack(sibling);
                    }
                }
            }
            else
            {
                if (parent.Color == NodeColor.BLACK && sibling.Color == NodeColor.BLACK)
                {
                    PullBlack(parent);
                    // 如果sibling有子节点，那么子节点一定是红色
                    if (sibling.LeftChild != null && sibling.RightChild != null)
                    {
                        RotateRight(parent);
                        SwapColors(parent, sibling);
                        PushBlack(sibling);
                    }
                    else if (sibling.LeftChild == null && sibling.RightChild == null)
                    {
                        FixTree4Deletion(parent, false);
                    }
                    else if (sibling.LeftChild != null)
                    {
                        // 和插入操作相同
                        RotateRight(parent);
                        SwapColors(sibling, parent);
                        // 将Double-Black push到两个红色节点即可达成修复
                        PushBlack(sibling);
                    }
                    else
                    {
                        // 和插入操作相同
                        RotateLeftRight(parent);
                        SwapColors(sibling.RightChild as TreeNode, parent);
                        // 将Double-Black push到两个红色节点即可达成修复
                        PushBlack(sibling.RightChild as TreeNode);
                    }
                }
                else if (parent.Color == NodeColor.RED && sibling.Color == NodeColor.BLACK)
                {
                    PullBlack(parent);
                    // 如果sibling有子节点，那么子节点一定是红色
                    if (sibling.LeftChild != null && sibling.RightChild != null)
                    {
                        RotateRight(parent);
                        SwapColors(parent, sibling);
                        PushBlack(parent);
                    }
                    else if (sibling.LeftChild == null && sibling.RightChild == null)
                    {
                        // 已经修复了，无需做事
                    }
                    else if (sibling.LeftChild != null)
                    {
                        RotateRight(parent);
                        SwapColors(sibling, parent);
                        PushBlack(sibling);
                    }
                    else
                    {
                        TreeNode siblingRight = sibling.RightChild as TreeNode;
                        RotateLeftRight(parent);
                        SwapColors(siblingRight, parent);
                        PushBlack(siblingRight);
                    }
                }
                else if (parent.Color == NodeColor.BLACK && sibling.Color == NodeColor.RED)
                {
                    // 此时sibling一定有两个黑色的子节点
                    RotateRight(parent);
                    SwapColors(parent, sibling);
                    FixTree4Deletion(node);
                }
            }
            if (remove)
            {
                // 最后移除这个Nil节点
                ReplaceNode(node, null);
            }
        }

        private void PullBlack(TreeNode node)
        {
            node.Color = node.Color + 1;
            TreeNode left = node.LeftChild as TreeNode;
            TreeNode right = node.RightChild as TreeNode;
            left.Color = left.Color - 1;
            right.Color = right.Color - 1;
        }

        private void PushBlack(TreeNode node)
        {
            node.Color = node.Color - 1;
            TreeNode left = node.LeftChild as TreeNode;
            TreeNode right = node.RightChild as TreeNode;
            left.Color = left.Color + 1;
            right.Color = right.Color + 1;
        }

        private TreeNode NodeToReplace(TreeNode node)
        {
            // 只需要考虑两种情况
            // 1. node为叶子节点
            // 2. node有一个子节点
            // 不用考虑node有两个子节点的情况是因为删除这种node需要找到后继节点，可以将删除操作转为删除后继节点，而后继节点只可能有右孩子
            if (node.Color == NodeColor.RED)
            {
                // 直接删除，所以返回空节点替换
                return null;
            }
            // 如果删除的是黑色的叶子结点
            if (node.LeftChild == null && node.RightChild == null)
            {
                // 删除后是一个Double-Black节点，需要修正成普通Black节点
                return TreeNode.DoubleBlackNilNode;
            }
            else if (node.LeftChild != null || node.RightChild != null)
            {
                // 为了满足红黑树的性质，子节点只可能是红色节点，并且只可能是叶子节点
                // 只需要将子节点涂成黑色替换即可
                TreeNode ret = null;
                if (node.LeftChild != null)
                {
                    ret = node.LeftChild as TreeNode;
                    ret.Color = NodeColor.BLACK;
                }
                else
                {
                    ret = node.RightChild as TreeNode;
                    ret.Color = NodeColor.BLACK;
                }
                return ret;
            }
            throw new ArgumentException("Node for deletion can't own two childs.");
        }

        private void SwapColors(TreeNode node1, TreeNode node2)
        {
            NodeColor tmp = node1.Color;
            node1.Color = node2.Color;
            node2.Color = tmp;
        }

        private TreeNode ReplaceNode(TreeNode orig, TreeNode dest)
        {
            if (dest == null)
            {
                if (orig.Parent == null)
                {
                    _root = dest;
                }
                else if (orig.IsLeftChild)
                {
                    orig.Parent.LeftChild = dest;
                }
                else
                {
                    orig.Parent.RightChild = dest;
                }
                return null;
            }
            else
            {
                orig.LeftChild = dest.LeftChild;
                orig.RightChild = dest.RightChild;
                orig.Data = dest.Data;
                orig.Color = dest.Color;
                return orig;
            }
        }

        private void RemoveNode(ITreeNode<T> node)
        {
            // 替换node的节点
            TreeNode toReplace = null;
            TreeNode replaced = null;
            if (node.LeftChild == null && node.RightChild == null)
            {
                toReplace = NodeToReplace(node as TreeNode);
                replaced = ReplaceNode(node as TreeNode, toReplace);
            }
            else if (node.LeftChild == null || node.RightChild == null)
            {
                toReplace = NodeToReplace(node as TreeNode);
                replaced = ReplaceNode(node as TreeNode, toReplace);
            }
            else
            {
                // 因为有右子树所以successor不可能为空
                var successor = FindSuccessor(node) as TreeNode;
                // 将successor的数据给node
                node.Data = successor.Data;
                // 转换为删除successor
                toReplace = NodeToReplace(successor);
                replaced = ReplaceNode(successor, toReplace);
            }
            --_size;

            // 如果存在Double-Black节点，需要修复红黑树
            if (replaced != null && replaced.Color == NodeColor.DOUBLE_BLACK)
            {
                FixTree4Deletion(replaced);
            }
        }

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
            var blackNodeCnt = new HashSet<Int32>();
            LevelTraversal(node =>
            {
                TreeNode avlNode = node as TreeNode;
                if (avlNode == _root && avlNode.Color != NodeColor.BLACK)
                {
                    throw new Exception("Invalid BSTree");
                }
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
                if (avlNode.LeftChild == null && avlNode.RightChild == null)
                {
                    Int32 blackCnt = 0;
                    var cur = avlNode;
                    while (cur != null)
                    {
                        if (cur.Color == NodeColor.BLACK)
                        {
                            blackCnt++;
                        }
                        cur = cur.Parent as TreeNode;
                    }
                    blackNodeCnt.Add(blackCnt);
                }
                if (avlNode.Parent != null && avlNode.Color == NodeColor.RED && (avlNode.Parent as TreeNode).Color == NodeColor.RED)
                {
                    throw new Exception("Invalid BSTree");
                }
                if (avlNode.Color == NodeColor.DOUBLE_BLACK)
                {
                    throw new Exception("Invalid BSTree");
                }
                ++chkSize;
            });
            if (chkSize != size)
            {
                throw new Exception("Invalid BSTree");
            }
            if (blackNodeCnt.Count > 1)
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
