using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public enum AnchorMovementType
    {
        /// <summary>
        /// 在锚点处插入文本时，锚点始终在插入文本之前
        /// </summary>
        BeforeInsertion,
        /// <summary>
        /// 在锚点处插入文本时，锚点始终在插入文本之后
        /// </summary>
        AfterInsertion,
    }

    /// <summary>
    /// 这里我称之为锚点，两个锚点组成一段文本；插入/删除文本的时候，会自动更新长度。
    /// </summary>
    /// <remarks>
    /// 在文本编辑中，偏移是会经常变化的，如果逐个去修改偏移的话，将会有非常大的计算量O(N)
    /// 所以这里用改良版的红黑树<see cref="TextAnchorTree"/>去对所有anchor进行维护，anchor用两个属性来快速计算偏移，Length和TotalLength。
    /// Length:指该anchor自身的长度（自身到前一个anchor位置的长度）
    /// TotalLength:指该anchor子树的总长度
    /// 每次添加/删除文本的时候，只需要修改之后/之前的属性，而且由于记录了TotalLength可以保证只需要遍历父节点而不需要遍历所有anchor，复杂度为O(log N)
    /// 计算偏移值也只需要找到所有左子树、父节点左子树的totallength以及自身length，就可以快速计算出偏移值，复杂度也为O(log N)
    /// </remarks>
    public sealed class TextAnchor
    {
        public Int32 Length { get; set; }
        public Int32 TotalLength { get; set; }

        #region Constructor
        internal TextAnchor()
        {
            Length = TotalLength = 0;
        }
        #endregion

        #region TreeNode Related
        internal enum NodeColor
        {
            RED = 0,
            BLACK = 1,
            DOUBLE_BLACK = 2,
        }

        internal static TextAnchor DoubleBlackNilNode
        {
            get { return new TextAnchor { Color = NodeColor.DOUBLE_BLACK, }; }
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
        internal TextAnchor LeftMost
        {
            get
            {
                TextAnchor node = this;
                while (node.Left != null)
                {
                    node = node.Left;
                }
                return node;
            }
        }
        internal TextAnchor RightMost
        {
            get
            {
                TextAnchor node = this;
                while (node.Right != null)
                {
                    node = node.Right;
                }
                return node;
            }
        }
        internal TextAnchor Predecessor
        {
            get
            {
                // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
                if (Left != null)
                {
                    return Left.RightMost;
                }
                // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
                TextAnchor node = this;
                while (node.IsLeft)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
        }
        internal TextAnchor Successor
        {
            get
            {
                // 存在右子树，则后继节点就是右子树最靠左的叶子节点
                if (Right != null)
                {
                    return Right.LeftMost;
                }
                // 不存在右子树，后继节点则为向上第一个右子树中不能存在该节点的节点
                TextAnchor node = this;
                while (node.IsRight)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
        }
        internal TextAnchor Parent { get; set; }
        internal TextAnchor Left { get; set; }
        internal TextAnchor Right { get; set; }
        internal NodeColor Color { get; set; }
        #endregion
    }
}
