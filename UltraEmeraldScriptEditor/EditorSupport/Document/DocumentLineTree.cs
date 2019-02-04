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
    /// 存储DocumentLine节点的红黑树，按照行号排序。
    /// </summary>
    /// <remarks>
    /// 大部分操作均为O(log N)
    /// 维护一系列<see cref="DocumentLineNode"/>节点。
    /// 节点的TotalCount表示包括自身的所有子节点的个数，TotalLength表示包括自身所有的子节点的总长度(_exactLength)
    /// TotalCount的数学意义：该节点覆盖的周围的子节点个数。
    /// TotalLength的数学意义：该节点覆盖的周围的子节点的总长度。
    /// 树永远不可能为空，起始为一个空行。
    /// </remarks>
    internal sealed class DocumentLineTree : IEnumerable<DocumentLine>
    {
        internal sealed class DocumentLineNode
        {
            internal static DocumentLineNode DoubleBlackNilNode
            {
                get { return new DocumentLineNode(null) { Color = NodeColor.DOUBLE_BLACK, }; }
            }

            internal DocumentLineNode(DocumentLine line)
            {
                TotalCount = 1;
                TotalLength = line != null ? line._exactLength : 0;
                Line = line;
                Color = NodeColor.RED;
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
            internal DocumentLineNode LeftMost
            {
                get
                {
                    DocumentLineNode node = this;
                    while (node.Left != null)
                    {
                        node = node.Left;
                    }
                    return node;
                }
            }
            internal DocumentLineNode RightMost
            {
                get
                {
                    DocumentLineNode node = this;
                    while (node.Right != null)
                    {
                        node = node.Right;
                    }
                    return node;
                }
            }
            internal DocumentLineNode Predecessor
            {
                get
                {
                    // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
                    if (Left != null)
                    {
                        return Left.RightMost;
                    }
                    // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
                    DocumentLineNode node = this;
                    while (node.IsLeft)
                    {
                        node = node.Parent;
                    }
                    return node.Parent;
                }
            }
            internal DocumentLineNode Successor
            {
                get
                {
                    // 存在右子树，则后继节点就是右子树最靠左的叶子节点
                    if (Right != null)
                    {
                        return Right.LeftMost;
                    }
                    // 不存在右子树，后继节点则为向上第一个右子树中不存在该节点的节点
                    DocumentLineNode node = this;
                    while (node.IsRight)
                    {
                        node = node.Parent;
                    }
                    return node.Parent;
                }
            }
            internal Boolean IsLeaf
            {
                get { return Left == null && Right == null; }
            }
            internal DocumentLineNode Parent { get; set; }
            internal DocumentLineNode Left { get; set; }
            internal DocumentLineNode Right { get; set; }
            internal NodeColor Color { get; set; }

            internal DocumentLine Line { get; set; }
            /// <summary>
            /// 所有子节点的个数（包括自身）
            /// </summary>
            internal Int32 TotalCount { get; set; }
            /// <summary>
            /// 子树的所有TotalLength，包括自身的line的_exactLength。
            /// </summary>
            internal Int32 TotalLength { get; set; }

            internal void Reset()
            {
                TotalCount = TotalLength = 0;
                Color = NodeColor.RED;
                Left = Right = Parent = null;
            }
        }

        #region Constructor
        internal DocumentLineTree(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
            Clear();
        }
        #endregion

        #region Line getters
        internal Int32 LineCount
        {
            get { return _root.TotalCount; }
        }

        public DocumentLine GetLineByNumber(Int32 number)
        {
            var node = GetNodeByIndex(number - 1);
            if (node != null)
            {
                return node.Line;
            }
            return null;
        }

        public DocumentLine GetLineByOffset(Int32 offset)
        {
            var node = GetNodeByOffset(offset);
            if (node != null)
            {
                return node.Line;
            }
            return null;
        }

        internal DocumentLineNode GetNodeByIndex(Int32 index)
        {
#if DEBUG
            VerifyIndexRange(index);
#endif
            DocumentLineNode curNode = _root;
            while (true)
            {
                if (curNode.Left != null)
                {
                    if (index < curNode.Left.TotalCount)
                    {
                        // 在左节点覆盖范围内
                        curNode = curNode.Left;
                        continue;
                    }
                    else
                    {
                        index = index - curNode.Left.TotalCount;
                    }
                }
                // 如果index是0，则当前节点即为所找
                if (index == 0)
                {
                    return curNode;
                }
                --index;
                // 去右节点继续寻找
                if (curNode.Right != null)
                {
                    curNode = curNode.Right;
                }
                else
                {
                    return null;
                }
            }
        }

        internal DocumentLineNode GetNodeByOffset(Int32 offset)
        {
#if DEBUG
            VerifyOffsetRange(offset);
#endif
            // 如果是最后位置，直接返回最后一行
            if (offset == _root.TotalLength)
            {
                return _root.RightMost;
            }
            DocumentLineNode curNode = _root;
            while (true)
            {
                if (curNode.Left != null)
                {
                    if (offset < curNode.Left.TotalLength)
                    {
                        // 在左节点覆盖范围内
                        curNode = curNode.Left;
                        continue;
                    }
                    else
                    {
                        offset -= curNode.Left.TotalLength;
                    }
                }
                if (offset < curNode.Line._exactLength)
                {
                    return curNode;
                }
                offset -= curNode.Line._exactLength;
                if (curNode.Right != null)
                {
                    curNode = curNode.Right;
                }
                else
                {
                    return null;
                }
            }
        }

        internal static Int32 GetIndexFromNode(DocumentLineNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            Int32 index = node.Left != null ? node.Left.TotalCount : 0;
            while (node.Parent != null)
            {
                DocumentLineNode prevNode = node;
                node = node.Parent;
                if (prevNode.IsRight)
                {
                    if (node.Left != null)
                    {
                        index += node.Left.TotalCount;
                    }
                    ++index;
                }
            }
            return index;
        }

        internal static Int32 GetOffsetFromNode(DocumentLineNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }
            Int32 offset = node.Left != null ? node.Left.TotalLength : 0;
            while (node.Parent != null)
            {
                DocumentLineNode prevNode = node;
                node = node.Parent;
                if (prevNode.IsRight)
                {
                    if (node.Left != null)
                    {
                        offset += node.Left.TotalLength;
                    }
                    offset += node.Line._exactLength;
                }
            }
            return offset;
        }

        private void VerifyIndexRange(Int32 index)
        {
            if (index < 0 || index >= LineCount)
            {
                throw new ArgumentOutOfRangeException("index", index, "0 <= index < " + LineCount.ToString(CultureInfo.InvariantCulture));
            }
        }

        private void VerifyOffsetRange(Int32 offset)
        {
            Int32 docLength = _doc.Length;
            if (offset < 0 || offset > docLength)
            {
                throw new ArgumentOutOfRangeException("offset", offset, "0 <= offset <= " + docLength.ToString(CultureInfo.InvariantCulture));
            }
        }
        #endregion

        #region Tree operations
        internal static Int32 GetHeightBySize(Int32 size)
        {
            if (size == 0)
            {
                return 0;
            }
            return GetHeightBySize(size / 2) + 1;
        }

        /// <summary>
        /// 直接创建一颗平衡的红黑树
        /// </summary>
        /// <param name="lines"></param>
        internal void RebuildTree(IList<DocumentLine> lines)
        {
            Debug.Assert(lines.Count > 0);
            var nodes = new DocumentLineNode[lines.Count];
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                var node = new DocumentLineNode(line);
                line._node = node;
                nodes[i] = node;
            }
            Int32 treeHeight = GetHeightBySize(nodes.Length);
            _root = BuildTree(nodes, 0, nodes.Length, treeHeight);
            _root.Color = NodeColor.BLACK;
#if DEBUG
            VerifySelf();
#endif
        }

        internal DocumentLineNode BuildTree(DocumentLineNode[] nodes, Int32 start, Int32 end, Int32 treeHeight)
        {
            Debug.Assert(start <= end);
            if (start == end)
            {
                return null;
            }
            Int32 middle = (start + end) / 2;
            DocumentLineNode middleNode = nodes[middle];
            middleNode.Left = BuildTree(nodes, start, middle, treeHeight - 1);
            middleNode.Right = BuildTree(nodes, middle + 1, end, treeHeight - 1);
            if (middleNode.Left != null)
            {
                middleNode.Left.Parent = middleNode;
            }
            if (middleNode.Right != null)
            {
                middleNode.Right.Parent = middleNode;
            }
            // 我们只把高度为1的节点染成红色，这样是最方便快捷的一颗红黑树
            middleNode.Color = treeHeight == 1 ? NodeColor.RED : NodeColor.BLACK;
            UpdateNodeData(middleNode);
            return middleNode;
        }

        internal void InsertLineAfter(DocumentLine line, DocumentLine newLine)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            if (newLine == null)
            {
                throw new ArgumentNullException("newLine");
            }
            var newNode = new DocumentLineNode(newLine);
            newLine._node = newNode;
            if (line._node.Right == null)
            {
                InsertAsRight(line._node, newNode);
            }
            else
            {
                InsertAsLeft(line._node.Successor, newNode);
            }
        }

        internal void RemoveLine(DocumentLine line)
        {
            if (line == null)
            {
                throw new ArgumentNullException("line");
            }
            RemoveNode(line._node);
        }

        internal void Clear()
        {
#if DEBUG
            var emptyLine = new DocumentLine(_doc);
#else
            var emptyLine = new DocumentLine();
#endif
            _root = new DocumentLineNode(emptyLine);
            _root.Color = NodeColor.BLACK;
            emptyLine._node = _root;
        }

        internal void InsertAsLeft(DocumentLineNode node, DocumentLineNode newNode)
        {
            Debug.Assert(node.Left == null);
            node.Left = newNode;
            newNode.Parent = node;
            UpdateNodeData(node);
            FixTree4Insertion(newNode);
        }

        internal void InsertAsRight(DocumentLineNode node, DocumentLineNode newNode)
        {
            Debug.Assert(node.Right == null);
            node.Right = newNode;
            newNode.Parent = node;
            UpdateNodeData(node);
            FixTree4Insertion(newNode);
        }

        internal void RemoveNode(DocumentLineNode node)
        {
            DocumentLineNode toReplace = null;
            DocumentLineNode replaced = null;
            DocumentLineNode successor = node.Successor;
            DocumentLineNode updateNode = null;
            Int32 nodeLength = node.Line._exactLength;
            if (node.Left != null && node.Right != null)
            {
                // 用后继节点替换
                node.Line = successor.Line;
                node.Line._node = node;
                node.Line._exactLength = successor.Line._exactLength;
                node.TotalLength = successor.TotalLength;
                node.TotalCount = successor.TotalCount;
                updateNode = successor.Parent;
                // 转换为删除successor
                toReplace = FindNodeToReplace(successor);
                replaced = ReplaceNode(successor, toReplace);
                successor = node;
            }
            else
            {
                toReplace = FindNodeToReplace(node);
                replaced = ReplaceNode(node, toReplace);
                if (toReplace == null)
                {
                    updateNode = node.Parent;
                }
                else
                {
                    if (toReplace == successor)
                    {
                        updateNode = node;
                        successor = node;
                    }
                    else
                    {
                        updateNode = node.Parent;
                    }
                }
            }

            if (updateNode != null)
            {
                UpdateNodeData(updateNode);
            }
            // 如果替换的节点是双黑节点，则需要修复
            if (replaced != null && replaced.Color == NodeColor.DOUBLE_BLACK)
            {
                FixTree4Deletion(replaced);
            }
            if (_root != null)
            {
                _root.Color = NodeColor.BLACK;
            }
        }

        internal void UpdateNodeData(DocumentLineNode node)
        {
            Int32 totalCnt = 1;
            Int32 totalLength = node.Line._exactLength;
            if (node.Left != null)
            {
                totalCnt += node.Left.TotalCount;
                totalLength += node.Left.TotalLength;
            }
            if (node.Right != null)
            {
                totalCnt += node.Right.TotalCount;
                totalLength += node.Right.TotalLength;
            }
            if (totalCnt != node.TotalCount || totalLength != node.TotalLength)
            {
                node.TotalCount = totalCnt;
                node.TotalLength = totalLength;
                if (node.Parent != null)
                {
                    UpdateNodeData(node.Parent);
                }
            }
        }
        #endregion

        #region Red-Black Tree Fix
        internal void FixTree4Insertion(DocumentLineNode node)
        {
            Debug.Assert(node.Color == NodeColor.RED);
            if (node.Parent == null)
            {
                // 根节点必须为黑色
                node.Color = NodeColor.BLACK;
                return;
            }
            DocumentLineNode parent = node.Parent;
            if (parent.Color == NodeColor.BLACK)
            {
                // 父节点为黑色则不需要修复
                return;
            }
            DocumentLineNode grandParent = parent.Parent;
            DocumentLineNode uncle = parent.IsLeft ? grandParent.Right : grandParent.Left;
            NodeColor uncleColor = GetNodeColor(uncle);
            if (uncleColor == NodeColor.RED)
            {
                // 如果uncle节点为红色，则只需要重新着色即可
                parent.Color = NodeColor.BLACK;
                uncle.Color = NodeColor.BLACK;
                // 多了一个黑色，将grandparent置为红色以平衡
                grandParent.Color = NodeColor.RED;
                // 继续修复grandParent
                FixTree4Insertion(grandParent);
            }
            else
            {
                // 如果uncle节点为黑色，此时grandParent一定为黑色
                // 1. node为左孩子，parent为左孩子，右旋grandParent
                if (node.IsLeft && parent.IsLeft)
                {
                    RotateRight(grandParent);
                    // 交换parent和grandParent的颜色
                    SwapColor(parent, grandParent);
                }
                // 2. node为左孩子，parent为右孩子，先右旋parent再左旋grandParent
                else if (node.IsLeft && parent.IsRight)
                {
                    RotateRight(parent);
                    // 变成右右的情况，parent和node变更
                    node = parent;
                    parent = node.Parent;
                    RotateLeft(grandParent);
                    SwapColor(parent, grandParent);
                }
                // 3. node为右孩子，parent为左孩子，先左旋parent再右旋grandParent
                else if (node.IsRight && parent.IsLeft)
                {
                    RotateLeft(parent);
                    // 变成左左的情况，parent和node变更
                    node = parent;
                    parent = node.Parent;
                    RotateRight(grandParent);
                    SwapColor(parent, grandParent);
                }
                // 4. node为右孩子，parent为右孩子，左旋grandParent
                else
                {
                    RotateLeft(grandParent);
                    // 交换parent和grandParent的颜色
                    SwapColor(parent, grandParent);
                }
                // 根节点一定为黑色
                _root.Color = NodeColor.BLACK;
            }
        }

        internal void FixTree4Deletion(DocumentLineNode node, Boolean remove = true)
        {
            // 其他情况已经在FindNodeToReplace中正确处理
            if (node.Color != NodeColor.DOUBLE_BLACK)
            {
                // 只可能是递归修复的时候，遇到父节点pull为黑色，这个时候已经修复完成
                return;
            }
            DocumentLineNode parent = node.Parent;
            if (parent == null)
            {
                // 如果根节点是双黑节点，变黑即可
                node.Color = NodeColor.BLACK;
                return;
            }
            DocumentLineNode sibling = node.IsLeft ? parent.Right : parent.Left;
            NodeColor siblingLeftColor = GetNodeColor(sibling.Left);
            NodeColor siblingRightColor = GetNodeColor(sibling.Right);
            if (sibling.Color == NodeColor.BLACK)
            {
                if (siblingLeftColor == NodeColor.BLACK && siblingRightColor == NodeColor.BLACK)
                {
                    // 如果sibling的孩子均为黑色
                    PullBlack(parent);
                    FixTree4Deletion(parent, false);
                }
                else
                {
                    // sibling的孩子至少有一个红色
                    PullBlack(parent);
                    if (node.IsLeft && siblingLeftColor == NodeColor.RED)
                    {
                        // sibling只有左孩子，先对sibling右旋，再对parent左旋
                        RotateRight(sibling);
                        // sibling变更
                        sibling = sibling.Parent;
                        RotateLeft(parent);
                        SwapColor(parent, sibling);
                    }
                    else if (node.IsLeft && siblingRightColor == NodeColor.RED)
                    {
                        // sibling只有右孩子，直接左旋parent
                        RotateLeft(parent);
                        SwapColor(parent, sibling);
                    }
                    else if (node.IsRight && siblingRightColor == NodeColor.RED)
                    {
                        RotateLeft(sibling);
                        sibling = sibling.Parent;
                        RotateRight(parent);
                        SwapColor(parent, sibling);
                    }
                    else
                    {
                        RotateRight(parent);
                        SwapColor(parent, sibling);
                    }
                    // 平衡颜色
                    PushBlack(sibling);
                }
            }
            else
            {
                // 我们需要想办法转到sibling为黑色的情况继续修复
                if (node.IsLeft)
                {
                    RotateLeft(parent);
                    SwapColor(parent, sibling);
                }
                else
                {
                    RotateRight(parent);
                    SwapColor(parent, sibling);
                }
                FixTree4Deletion(node, false);
            }
            if (remove)
            {
                // 将辅助用的双黑节点置空
                ReplaceNode(node, null);
            }
        }

        private void PullBlack(DocumentLineNode node)
        {
            Debug.Assert(node.Left != null && node.Right != null);
            node.Color = node.Color + 1;
            DocumentLineNode left = node.Left;
            DocumentLineNode right = node.Right;
            left.Color = left.Color - 1;
            right.Color = right.Color - 1;
        }

        private void PushBlack(DocumentLineNode node)
        {
            Debug.Assert(node.Left != null && node.Right != null);
            node.Color = node.Color - 1;
            DocumentLineNode left = node.Left;
            DocumentLineNode right = node.Right;
            left.Color = left.Color + 1;
            right.Color = right.Color + 1;
        }

        internal void RotateLeft(DocumentLineNode node)
        {
            /* 左旋
             *         N            R
             *        / \          / \
             *       L   R  ===>  N   T1
             *          / \      / \
             *         T  T1    L   T
             */
            DocumentLineNode left = node.Left;
            DocumentLineNode right = node.Right;
            DocumentLineNode t = right.Left;
            DocumentLineNode parent = node.Parent;

            right.Parent = parent;
            if (node.IsLeft)
            {
                parent.Left = right;
            }
            else if (node.IsRight)
            {
                parent.Right = right;
            }
            node.Parent = right;
            right.Left = node;
            node.Right = t;
            if (t != null)
            {
                t.Parent = node;
            }
            // 是否需要更换根节点
            if (parent == null)
            {
                _root = right;
            }
            // 更新锚点覆盖范围
            UpdateNodeData(node);
            UpdateNodeData(right);
        }

        internal void RotateRight(DocumentLineNode node)
        {
            /* 右旋
             *         N            L
             *        / \          / \
             *       L   R  ===>  T1  N
             *      / \              / \
             *     T1  T            T   R
             */
            DocumentLineNode left = node.Left;
            DocumentLineNode right = node.Right;
            DocumentLineNode t = left.Right;
            DocumentLineNode parent = node.Parent;

            left.Parent = parent;
            if (node.IsLeft)
            {
                parent.Left = left;
            }
            else if (node.IsRight)
            {
                parent.Right = left;
            }
            node.Parent = left;
            left.Right = node;
            node.Left = t;
            if (t != null)
            {
                t.Parent = node;
            }
            // 是否需要更换根节点
            if (parent == null)
            {
                _root = left;
            }
            // 更新锚点覆盖范围
            UpdateNodeData(node);
            UpdateNodeData(left);
        }

        internal void SwapColor(DocumentLineNode node1, DocumentLineNode node2)
        {
            NodeColor tmp = node1.Color;
            node1.Color = node2.Color;
            node2.Color = tmp;
        }

        internal DocumentLineNode ReplaceNode(DocumentLineNode orig, DocumentLineNode dest)
        {
            Debug.Assert(orig != dest);
            DocumentLineNode parent = orig.Parent;
            if (dest == null)
            {
                if (parent == null)
                {
                    _root = dest;
                }
                else if (orig.IsLeft)
                {
                    parent.Left = dest;
                }
                else
                {
                    parent.Right = dest;
                }
                return dest;
            }
            else
            {
                orig.Left = dest.Left;
                orig.Right = dest.Right;
                orig.Color = dest.Color;
                orig.Line = dest.Line;
                if (orig.Line != null)
                {
                    // dest可能为双黑节点，双黑节点没有anchor
                    orig.Line._node = orig;
                }
                orig.TotalCount = dest.TotalCount;
                orig.TotalLength = dest.TotalLength;
                return orig;
            }
        }

        internal DocumentLineNode FindNodeToReplace(DocumentLineNode node)
        {
            Debug.Assert(node.Left == null || node.Right == null);
            // 如果是根节点直接用nil节点替换
            if (node.Parent == null && node.IsLeaf)
            {
                return null;
            }
            // node只需要考虑两种情况
            // 1. node为叶子节点
            // 2. node为左孩子或右孩子
            // 不需要考虑node有两个孩子的情况是因为这个时候只需要找到node的后继节点替换，而后继节点只可能有一个右孩子
            if (node.Color == NodeColor.RED)
            {
                // 如果删除的是红色节点，直接用nil节点替换即可
                return null;
            }
            if (node.IsLeaf)
            {
                // 删除后是一个Double-Black节点，需要修复
                return DocumentLineNode.DoubleBlackNilNode;
            }
            else
            {
                DocumentLineNode ret = null;
                // 子节点只可能是红色节点，并且一定是叶子节点，直接涂黑替换即可
                if (node.Left != null)
                {
                    ret = node.Left;
                }
                else
                {
                    ret = node.Right;
                }
                ret.Color = NodeColor.BLACK;
                return ret;
            }
        }

        internal NodeColor GetNodeColor(DocumentLineNode node)
        {
            if (node == null)
            {
                // NIL节点均视为黑色
                return NodeColor.BLACK;
            }
            return node.Color;
        }
        #endregion

        #region IEnumerable<DocumentLine>
        public IEnumerator<DocumentLine> GetEnumerator()
        {
            return Enumerate();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerator<DocumentLine> Enumerate()
        {
            DocumentLineNode node = _root.LeftMost;
            DocumentLine line = node.Line;
            while (line != null)
            {
                yield return line;
                line = line.NextLine;
            }
        }
        #endregion

        #region Debug
        [Conditional("DEBUG")]
        /// <summary>
        /// 层次遍历
        /// </summary>
        /// <param name="action"></param>
        internal void LevelTraversal(Action<DocumentLineNode> action)
        {
            if (_root == null || action == null)
            {
                return;
            }
            var nodeQueue = new Queue<DocumentLineNode>();
            nodeQueue.Enqueue(_root);
            while (nodeQueue.Count > 0)
            {
                var curNode = nodeQueue.Dequeue();
                action(curNode);
                if (curNode.Left != null)
                {
                    nodeQueue.Enqueue(curNode.Left);
                }
                if (curNode.Right != null)
                {
                    nodeQueue.Enqueue(curNode.Right);
                }
            }
        }

        [Conditional("DEBUG")]
        internal void VerifySelf()
        {
            var blackNodeCnt = new HashSet<Int32>();
            Int32 totalLen = 0, totalCnt = 0;
            LevelTraversal(node =>
            {
                if (node.Color == NodeColor.DOUBLE_BLACK)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                if (node == _root && node.Color != NodeColor.BLACK)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                if (node.Parent == null && node != _root)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                if (node.Parent != null && !node.IsLeft && !node.IsRight)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                if (node.Left == null && node.Right == null)
                {
                    Int32 blackCnt = 0;
                    var cur = node;
                    while (cur != null)
                    {
                        if (cur.Color == NodeColor.BLACK)
                        {
                            blackCnt++;
                        }
                        cur = cur.Parent;
                    }
                    blackNodeCnt.Add(blackCnt);
                }
                if (node.Parent != null && node.Color == NodeColor.RED && node.Parent.Color == NodeColor.RED)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                Int32 leftLen = node.Left == null ? 0 : node.Left.TotalLength;
                Int32 rightLen = node.Right == null ? 0 : node.Right.TotalLength;
                if (node.TotalLength != node.Line._exactLength + leftLen + rightLen)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                totalLen += node.Line._exactLength;
                Int32 leftCnt = node.Left == null ? 0 : node.Left.TotalCount;
                Int32 rightCnt = node.Right == null ? 0 : node.Right.TotalCount;
                if (node.TotalCount != 1 + leftCnt + rightCnt)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                if (node.Line._node != node)
                {
                    throw new Exception("Invalid DocumentLineTree");
                }
                totalCnt += 1;
            });
            if (blackNodeCnt.Count > 1)
            {
                throw new Exception("Invalid DocumentLineTree");
            }
            if (_root != null && totalLen != _root.TotalLength)
            {
                throw new Exception("Invalid DocumentLineTree");
            }
            if (_root != null && totalCnt != _root.TotalCount)
            {
                throw new Exception("Invalid DocumentLineTree");
            }
        }
        #endregion

        private DocumentLineNode _root;
        private TextDocument _doc;
    }
}
