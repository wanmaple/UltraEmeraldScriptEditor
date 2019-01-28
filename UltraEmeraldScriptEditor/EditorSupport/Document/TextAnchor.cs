using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public enum AnchorMovementType
    {
        BeforeInsertion,
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

        public Int32 Offset
        {
            get
            {
                Debug.Assert(_node != null);
                Int32 offset = 0;
                TextAnchorTree.TextAnchorNode curNode = _node;
                TextAnchorTree.TextAnchorNode prevNode = null;
                do
                {
                    offset += curNode.Length;
                    if (curNode.Left != null)
                    {
                        offset += curNode.Left.TotalLength;
                    }
                    do
                    {
                        prevNode = curNode;
                        curNode = curNode.Parent;
                        if (curNode == null || prevNode.IsRight)
                        {
                            break;
                        }
                    } while (true);
                } while (curNode != null);
                return offset;
            }
        }

        internal AnchorMovementType MovementType
        {
            get { return _movementType; }
            set { _movementType = value; }
        }

        internal Boolean Alive
        {
            get { return _alive; }
            set { _alive = value; }
        }

        private Boolean _alive;
        private AnchorMovementType _movementType;

        internal TextAnchorTree.TextAnchorNode _node;
        internal TextDocument _doc;
    }
}
