using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using EditorSupport.Editing;

namespace EditorSupport.CodeCompletion
{
    /// <summary>
    /// 智能提示框。
    /// </summary>
    public class CompletionWindow : CompletionWindowBase
    {
        static CompletionWindow()
        {
            WidthProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(200.0));
            HeightProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(150.0));
        }

        public CompletionWindow(EditView editview, IEnumerable<ICompletionData> completions) 
            : base(editview)
        {
            if (completions == null)
            {
                throw new ArgumentNullException("completions");
            }
            _allCompletions = new ObservableCollection<ICompletionData>(completions);
            _filteredCompletions = new ObservableCollection<ICompletionData>(completions);
            _completionList = new CompletionList();
            _completionList.Completions = _filteredCompletions;
            Content = _completionList;
        }

        protected CompletionList _completionList;
        protected ObservableCollection<ICompletionData> _allCompletions;
        protected ObservableCollection<ICompletionData> _filteredCompletions;
    }
}
