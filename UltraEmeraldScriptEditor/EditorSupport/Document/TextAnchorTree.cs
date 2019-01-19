using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 存储TextAnchor节点的改良版红黑树，该红黑树并不会自己排序，需要手动插入或删除，主要是利用红黑树的自平衡性质
    /// </summary>
    public sealed class TextAnchorTree
    {
        private TextAnchor _root;
    }
}
