using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UltraEmeraldScriptEditor.DataStructure
{
    /// <summary>
    /// 二叉树节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITreeNode<T>
    {
        Boolean IsLeftChild { get; }
        Boolean IsRightChild { get; }
        T Data { get; set; }
        ITreeNode<T> Parent { get; set; }
        ITreeNode<T> LeftChild { get; set; }
        ITreeNode<T> RightChild { get; set; }
    }
}
