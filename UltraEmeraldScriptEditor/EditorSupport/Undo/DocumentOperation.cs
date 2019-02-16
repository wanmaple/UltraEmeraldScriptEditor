using EditorSupport.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Undo
{
    public abstract class DocumentOperation : IUndoableOperation
    {
        #region Abstraction
        public abstract void Redo();

        public abstract void Undo();
        #endregion

        public TextDocument Document => _document;

        protected DocumentOperation(TextDocument document)
        {
            _document = document ?? throw new ArgumentNullException("document");
        }

        protected TextDocument _document;
    }
}
