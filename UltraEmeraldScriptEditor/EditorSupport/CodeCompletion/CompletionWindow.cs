using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EditorSupport.Editing;
using EditorSupport.Utils;

namespace EditorSupport.CodeCompletion
{
    /// <summary>
    /// 智能提示框。
    /// </summary>
    public class CompletionWindow : CompletionWindowBase
    {
        public ObservableCollection<ICompletionData> Completions => _allCompletions;

        static CompletionWindow()
        {
            WidthProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(200.0));
            HeightProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(120.0));
        }

        public CompletionWindow() 
            : base()
        {
            _allCompletions = new ObservableCollection<ICompletionData>();
            _completionList = new CompletionList();
            _completionList.Completions = _allCompletions;
            Content = _completionList;
        }

        protected CompletionList _completionList;
        protected ObservableCollection<ICompletionData> _allCompletions;
    }
}
