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
    public sealed class TextAnchor
    {
        #region Constructor
        internal TextAnchor(TextDocument document)
        {
            _doc = document ?? throw new ArgumentNullException("document");
        }
        #endregion

        internal TextAnchorTree.TextAnchorNode _node;
        internal TextDocument _doc;
    }
}
