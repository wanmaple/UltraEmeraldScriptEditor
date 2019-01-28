using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    internal enum NodeColor
    {
        RED = 0,
        BLACK = 1,
        DOUBLE_BLACK = 2,
    }

    /// <summary>
    /// 存储TextAnchor节点的改良版红黑树，该红黑树是按照实际的offset排序。
    /// </summary>
    /// <remarks>
    /// 在文本编辑中，偏移是会经常变化的，如果逐个去修改偏移的话，将会有非常大的计算量O(N)
    /// 所以这里用改良版的红黑树去对所有anchor进行维护，<see cref="TextAnchorNode"/>用两个属性来快速计算偏移，Length和TotalLength。
    /// Length:指该anchor自身的长度（自身到前一个anchor位置的长度）
    /// TotalLength:指该anchor子树的总长度（包含自身的长度）
    /// TotalLength的数学意义是该锚点所覆盖的所有子锚点（把该锚点想象成它周围其他子锚点的管理者）的总长度
    /// 每次插入新锚点的时候，只需要找到靠后的最近一个锚点，将新锚点设置为该锚点的前驱，而且由于记录了TotalLength可以快速找到偏移的位置，复杂度为O(log N)
    /// 计算偏移值也只需要找到所有左子树、父节点左子树的totallength以及自身length，就可以快速计算出偏移值，复杂度也为O(log N)
    /// 不能存在重复偏移的锚点
    /// </remarks>
    internal sealed class TextAnchorTree
    {
        internal sealed class TextAnchorNode
        {
            internal static TextAnchorNode DoubleBlackNilNode
            {
                get { return new TextAnchorNode(null) { Color = NodeColor.DOUBLE_BLACK, }; }
            }

            internal TextAnchorNode(TextAnchor anchor)
            {
                Anchor = anchor;
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
            internal TextAnchorNode LeftMost
            {
                get
                {
                    TextAnchorNode node = this;
                    while (node.Left != null)
                    {
                        node = node.Left;
                    }
                    return node;
                }
            }
            internal TextAnchorNode RightMost
            {
                get
                {
                    TextAnchorNode node = this;
                    while (node.Right != null)
                    {
                        node = node.Right;
                    }
                    return node;
                }
            }
            internal TextAnchorNode Predecessor
            {
                get
                {
                    // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
                    if (Left != null)
                    {
                        return Left.RightMost;
                    }
                    // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
                    TextAnchorNode node = this;
                    while (node.IsLeft)
                    {
                        node = node.Parent;
                    }
                    return node.Parent;
                }
            }
            internal TextAnchorNode Successor
            {
                get
                {
                    // 存在右子树，则后继节点就是右子树最靠左的叶子节点
                    if (Right != null)
                    {
                        return Right.LeftMost;
                    }
                    // 不存在右子树，后继节点则为向上第一个右子树中不存在该节点的节点
                    TextAnchorNode node = this;
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
            internal TextAnchorNode Parent { get; set; }
            internal TextAnchorNode Left { get; set; }
            internal TextAnchorNode Right { get; set; }
            internal NodeColor Color { get; set; }

            public Int32 Length { get; set; }
            public Int32 TotalLength { get; set; }
            internal TextAnchor Anchor { get; set; }
        }

        #region Constructor
        internal TextAnchorTree(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
        }
        #endregion

        #region Tree operations
        internal TextAnchor CreateAnchor(Int32 offset)
        {
            var anchor = new TextAnchor(_doc);
            anchor._node = new TextAnchorNode(anchor);
            if (_root == null)
            {
                // 第一个锚点
                _root = anchor._node;
                _root.TotalLength = _root.Length = offset;
                FixTree4Insertion(_root);
            }
            else if (offset >= _root.TotalLength)
            {
                // 超过了整棵树覆盖的范围，所以直接加入尾部
                anchor._node.TotalLength = anchor._node.Length = offset - _root.TotalLength;
                InsertAsRight(_root.RightMost, anchor._node);
            }
            else
            {
                // 锚点在整棵树覆盖的范围内，先找到该offset靠后的最近锚点
                TextAnchorNode nearestAfter = FindNode(_root, ref offset);
                // 找不到后置锚点的情况已经在前面一个分支处理过了
                Debug.Assert(nearestAfter != null);
                anchor._node.TotalLength = anchor._node.Length = offset;
                nearestAfter.Length -= offset;
                // 在后置锚点前插入这个锚点，该锚点就是后置锚点的前驱
                if (nearestAfter.Left == null)
                {
                    InsertAsLeft(nearestAfter, anchor._node);
                }
                else
                {
                    InsertAsRight(nearestAfter.Predecessor, anchor._node);
                }
            }
#if DEBUG
            VerifySelf();
#endif
            return anchor;
        }

        internal void RemoveAnchor(TextAnchor anchor)
        {
            RemoveNode(anchor._node);
        }

        /// <summary>
        /// 插入文本的时候修复Anchor的偏移
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="ahchorMoveAfterInsertion">表示如果offset正好为anchor的偏移时，anchor是前移还是后移</param>
        internal void InsertText(Int32 offset, Int32 length, Boolean ahchorMoveAfterInsertion = true)
        {

        }

        internal void RemoveText(Int32 offset, Int32 length)
        {

        }

        internal void RemoveNode(TextAnchorNode node)
        {
            TextAnchorNode toReplace = null;
            TextAnchorNode replaced = null;
            TextAnchorNode successor = node.Successor;
            TextAnchorNode updateNode = null;
            Int32 nodeLength = node.Length;
            if (node.Left != null && node.Right != null)
            {
                // 用后继节点替换
                node.Anchor = successor.Anchor;
                node.Anchor._node = node;
                node.Length = successor.Length;
                node.TotalLength = successor.TotalLength;
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

            // 后继节点长度增加
            if (successor != null)
            {
                successor.Length += nodeLength;
            }
            if (updateNode != null)
            {
                UpdateTotalLength(updateNode);
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
#if DEBUG
            VerifySelf();
#endif
        }

        /// <summary>
        /// 寻找node下offset处的后置锚点，offset会被修改为该节点的相对偏移
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal TextAnchorNode FindNode(TextAnchorNode node, ref Int32 offset)
        {
            while (true)
            {
                // 如果存在左侧锚点
                if (node.Left != null)
                {
                    // offset在左锚点覆盖范围内
                    if (offset < node.Left.TotalLength)
                    {
                        // 继续判断offset是否在更左侧
                        node = node.Left;
                        continue;
                    }
                    // offset在右锚点覆盖范围内
                    else
                    {
                        // 转为相对该锚点的偏移
                        offset -= node.Left.TotalLength;
                    }
                }
                // 如果相对的offset已经在当前锚点的左侧，那么该节点就是要找的节点（因为当前锚点的右锚点都在更后面）
                Debug.Assert(offset != node.Length);
                if (offset < node.Length)
                {
                    return node;
                }
                // 在当前节点右侧，则继续转成相对右锚点的相对偏移
                offset -= node.Length;
                if (node.Right != null)
                {
                    // 在右锚点覆盖范围内，继续寻找
                    node = node.Right;
                }
                else
                {
                    // 如果没有右锚点了，说明不存在该offset的后置锚点
                    return null;
                }
            }
        }

        internal void InsertAsLeft(TextAnchorNode node, TextAnchorNode newNode)
        {
            Debug.Assert(node.Left == null);
            node.Left = newNode;
            newNode.Parent = node;
            UpdateTotalLength(node);
            FixTree4Insertion(newNode);
        }

        internal void InsertAsRight(TextAnchorNode node, TextAnchorNode newNode)
        {
            Debug.Assert(node.Right == null);
            node.Right = newNode;
            newNode.Parent = node;
            UpdateTotalLength(node);
            FixTree4Insertion(newNode);
        }

        internal void UpdateTotalLength(TextAnchorNode node)
        {
            Int32 totalLength = node.Length;
            if (node.Left != null)
            {
                totalLength += node.Left.TotalLength;
            }
            if (node.Right != null)
            {
                totalLength += node.Right.TotalLength;
            }
            if (node.TotalLength != totalLength)
            {
                // 如果总长度发生变化，则不断更新父节点的总长度
                node.TotalLength = totalLength;
                if (node.Parent != null)
                {
                    UpdateTotalLength(node.Parent);
                }
            }
        } 
        #endregion

        #region Red-Black Tree Fix
        internal void FixTree4Insertion(TextAnchorNode node)
        {
            Debug.Assert(node.Color == NodeColor.RED);
            if (node.Parent == null)
            {
                // 根节点必须为黑色
                node.Color = NodeColor.BLACK;
                return;
            }
            TextAnchorNode parent = node.Parent;
            if (parent.Color == NodeColor.BLACK)
            {
                // 父节点为黑色则不需要修复
                return;
            }
            TextAnchorNode grandParent = parent.Parent;
            TextAnchorNode uncle = parent.IsLeft ? grandParent.Right : grandParent.Left;
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

        internal void FixTree4Deletion(TextAnchorNode node, Boolean remove = true)
        {
            // 其他情况已经在FindNodeToReplace中正确处理
            if (node.Color != NodeColor.DOUBLE_BLACK)
            {
                // 只可能是递归修复的时候，遇到父节点pull为黑色，这个时候已经修复完成
                return;
            }
            TextAnchorNode parent = node.Parent;
            if (parent == null)
            {
                // 如果根节点是双黑节点，变黑即可
                node.Color = NodeColor.BLACK;
                return;
            }
            TextAnchorNode sibling = node.IsLeft ? parent.Right : parent.Left;
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

        private void PullBlack(TextAnchorNode node)
        {
            Debug.Assert(node.Left != null && node.Right != null);
            node.Color = node.Color + 1;
            TextAnchorNode left = node.Left;
            TextAnchorNode right = node.Right;
            left.Color = left.Color - 1;
            right.Color = right.Color - 1;
        }

        private void PushBlack(TextAnchorNode node)
        {
            Debug.Assert(node.Left != null && node.Right != null);
            node.Color = node.Color - 1;
            TextAnchorNode left = node.Left;
            TextAnchorNode right = node.Right;
            left.Color = left.Color + 1;
            right.Color = right.Color + 1;
        }

        internal void RotateLeft(TextAnchorNode node)
        {
            /* 左旋
             *         N            R
             *        / \          / \
             *       L   R  ===>  N   T1
             *          / \      / \
             *         T  T1    L   T
             */
            TextAnchorNode left = node.Left;
            TextAnchorNode right = node.Right;
            TextAnchorNode t = right.Left;
            TextAnchorNode parent = node.Parent;

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
            UpdateTotalLength(node);
            UpdateTotalLength(right);
        }

        internal void RotateRight(TextAnchorNode node)
        {
            /* 右旋
             *         N            L
             *        / \          / \
             *       L   R  ===>  T1  N
             *      / \              / \
             *     T1  T            T   R
             */
            TextAnchorNode left = node.Left;
            TextAnchorNode right = node.Right;
            TextAnchorNode t = left.Right;
            TextAnchorNode parent = node.Parent;

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
            UpdateTotalLength(node);
            UpdateTotalLength(left);
        }

        internal void SwapColor(TextAnchorNode node1, TextAnchorNode node2)
        {
            NodeColor tmp = node1.Color;
            node1.Color = node2.Color;
            node2.Color = tmp;
        }

        internal TextAnchorNode ReplaceNode(TextAnchorNode orig, TextAnchorNode dest)
        {
            Debug.Assert(orig != dest);
            TextAnchorNode parent = orig.Parent;
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
                orig.Anchor = dest.Anchor;
                if (orig.Anchor != null)
                {
                    // dest可能为双黑节点，双黑节点没有anchor
                    orig.Anchor._node = orig;
                }
                orig.Length = dest.Length;
                orig.TotalLength = dest.TotalLength;
                return orig;
            }
        }

        internal TextAnchorNode FindNodeToReplace(TextAnchorNode node)
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
                return TextAnchorNode.DoubleBlackNilNode;
            }
            else
            {
                TextAnchorNode ret = null;
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

        internal NodeColor GetNodeColor(TextAnchorNode node)
        {
            if (node == null)
            {
                // NIL节点均视为黑色
                return NodeColor.BLACK;
            }
            return node.Color;
        }
        #endregion

        #region Debug
        [Conditional("DEBUG")]
        /// <summary>
        /// 层次遍历
        /// </summary>
        /// <param name="action"></param>
        internal void LevelTraversal(Action<TextAnchorNode> action)
        {
            if (_root == null || action == null)
            {
                return;
            }
            var nodeQueue = new Queue<TextAnchorNode>();
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
            Int32 totalLen = 0;
            LevelTraversal(node =>
            {
                if (node.Color == NodeColor.DOUBLE_BLACK)
                {
                    throw new Exception("Invalid TextAnchorTree");
                }
                if (node == _root && node.Color != NodeColor.BLACK)
                {
                    throw new Exception("Invalid TextAnchorTree");
                }
                if (node.Parent == null && node != _root)
                {
                    throw new Exception("Invalid TextAnchorTree");
                }
                if (node.Parent != null && !node.IsLeft && !node.IsRight)
                {
                    throw new Exception("Invalid TextAnchorTree");
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
                    throw new Exception("Invalid TextAnchorTree");
                }
                Int32 leftLen = node.Left == null ? 0 : node.Left.TotalLength;
                Int32 rightLen = node.Right == null ? 0 : node.Right.TotalLength;
                if (node.TotalLength != node.Length + leftLen + rightLen)
                {
                    throw new Exception("Invalid TextAnchorTree");
                }
                if (node.Anchor._node != node)
                {
                    throw new Exception("Invalid TextAnchorTree");
                }
                totalLen += node.Length;
            });
            if (blackNodeCnt.Count > 1)
            {
                throw new Exception("Invalid TextAnchorTree");
            }
            if (_root != null && totalLen != _root.TotalLength)
            {
                throw new Exception("Invalid TextAnchorTree");
            }
        }
        #endregion

        private TextAnchorNode _root;
        private TextDocument _doc;
    }
}
