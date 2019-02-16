using EditorSupport.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace EditorSupport.CodeCompletion
{
    public class CompletionWindowBase : Window
    {
        static CompletionWindowBase()
        {
            WindowStyleProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(WindowStyle.None));
            ShowActivatedProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(false));
            ShowInTaskbarProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(false));
        }

        protected CompletionWindowBase(EditView editview)
        {
            _editview = editview ?? throw new ArgumentNullException("editview");
        }

        protected EditView _editview;
    }
}
