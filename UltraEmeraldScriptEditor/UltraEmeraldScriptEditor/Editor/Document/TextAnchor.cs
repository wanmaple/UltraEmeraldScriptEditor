using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UltraEmeraldScriptEditor.DataStructure;

namespace UltraEmeraldScriptEditor.Editor.Document
{
    /// <summary>
    /// 一段文本，也标志着一段偏移（初始和结束），在该偏移之前插入/删除文本的时候，会自动更新偏移值。
    /// </summary>
    /// <remarks>
    /// 在文本编辑中，偏移是会经常变化的，如果逐个去修改偏移的话，将会有非常大的计算量O(N)
    /// 所以这里用红黑树去对所有anchor进行维护，anchor用两个属性来快速计算偏移，Length和TotalLength。
    /// Length:指该anchor自身的长度（自身到前一个anchor位置的长度）
    /// TotalLength:指该anchor所有子节点的总长度
    /// 每次添加/删除文本的时候，只需要修改之后/之前的属性，而且由于记录了TotalLength可以保证不需要遍历所有anchor，复杂度为O(log N)
    /// 计算偏移值也只需要找到所有左子树、父节点左子树的totallength以及自身length，就可以快速计算出偏移值，复杂度也为O(log N)
    /// </remarks>
    public class TextAnchor
    {
        public Int32 Length { get; set; }
        public Int32 TotalLength { get; set; }

        public TextAnchor()
        {
            Length = TotalLength = 0;
        }

        #region TreeNode Related
        public enum NodeColor
        {
            RED = 0,
            BLACK = 1,
            DOUBLE_BLACK = 2,
        }

        public static TextAnchor DoubleBlackNilNode
        {
            get { return new TextAnchor { Color = NodeColor.DOUBLE_BLACK, }; }
        }
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
        public TextAnchor LeftMost
        {
            get
            {
                TextAnchor node = this;
                while (node.LeftChild != null)
                {
                    node = node.LeftChild;
                }
                return node;
            }
        }
        public TextAnchor RightMost
        {
            get
            {
                TextAnchor node = this;
                while (node.RightChild != null)
                {
                    node = node.RightChild;
                }
                return node;
            }
        }
        public TextAnchor Predecessor
        {
            get
            {
                // 存在左子树，则前驱节点就是左子树最靠右的叶子节点
                if (LeftChild != null)
                {
                    return LeftChild.RightMost;
                }
                // 不存在左子树，前驱节点则为向上第一个左子树中不存在该节点的节点
                TextAnchor node = this;
                while (node.Parent != null && node.Parent.LeftChild == node)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
        }
        public TextAnchor Successor
        {
            get
            {
                // 存在右子树，则后继节点就是右子树最靠左的叶子节点
                if (RightChild != null)
                {
                    return RightChild.LeftMost;
                }
                // 不存在右子树，后继节点则为向上第一个右子树中不能存在该节点的节点
                TextAnchor node = this;
                while (node.Parent != null && node.Parent.RightChild == node)
                {
                    node = node.Parent;
                }
                return node.Parent;
            }
        }
        public TextAnchor Parent { get; set; }
        public TextAnchor LeftChild { get; set; }
        public TextAnchor RightChild { get; set; }
        public NodeColor Color { get; set; } 
        #endregion
    }
}
