using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public class DocumentUpdateEventArgs : EventArgs
    {
        public ReadOnlyCollection<DocumentUpdate> Updates => _readonly;

        public DocumentUpdateEventArgs(DocumentUpdate update)
            : this(new DocumentUpdate[] { update, })
        {
        }
        public DocumentUpdateEventArgs(IEnumerable<DocumentUpdate> updates)
        {
            _updates = new List<DocumentUpdate>();
            _updates.AddRange(updates);
            _readonly = new ReadOnlyCollection<DocumentUpdate>(_updates);
        }

        private List<DocumentUpdate> _updates;
        private ReadOnlyCollection<DocumentUpdate> _readonly;
    }
}
