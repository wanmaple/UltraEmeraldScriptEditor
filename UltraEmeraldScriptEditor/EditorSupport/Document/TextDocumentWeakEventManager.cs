using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public static class TextDocumentWeakEventManager
    {
        public sealed class Changing : WeakEventManagerBase<TextDocument, Changing>
        {
            protected override void StartListening(TextDocument publisher)
            {
                publisher.Changing += base.DeliverEvent;
            }

            protected override void StopListening(TextDocument publisher)
            {
                publisher.Changing -= base.DeliverEvent;
            }
        }

        public sealed class Changed : WeakEventManagerBase<TextDocument, Changed>
        {
            protected override void StartListening(TextDocument publisher)
            {
                publisher.Changed += base.DeliverEvent;
            }

            protected override void StopListening(TextDocument publisher)
            {
                publisher.Changed -= base.DeliverEvent;
            }
        }

        public sealed class UpdateStarted : WeakEventManagerBase<TextDocument, UpdateStarted>
        {
            protected override void StartListening(TextDocument publisher)
            {
                publisher.UpdateStarted += base.DeliverEvent;
            }

            protected override void StopListening(TextDocument publisher)
            {
                publisher.UpdateStarted -= base.DeliverEvent;
            }
        }

        public sealed class UpdateFinished : WeakEventManagerBase<TextDocument, UpdateFinished>
        {
            protected override void StartListening(TextDocument publisher)
            {
                publisher.UpdateFinished += base.DeliverEvent;
            }

            protected override void StopListening(TextDocument publisher)
            {
                publisher.UpdateFinished -= base.DeliverEvent;
            }
        }
    }
}
