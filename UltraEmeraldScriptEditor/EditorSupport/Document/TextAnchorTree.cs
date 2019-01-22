using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 存储TextAnchor节点的改良版红黑树，该红黑树并不会自己排序，需要手动插入或删除，主要是利用红黑树的自平衡性质
    /// </summary>
    internal sealed class TextAnchorTree
    {
        public TextAnchorTree(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
        }

        private TextAnchor _root;
        private TextDocument _doc;
    }
}
