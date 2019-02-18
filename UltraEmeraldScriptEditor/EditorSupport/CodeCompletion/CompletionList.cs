using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EditorSupport.CodeCompletion
{
    public class CompletionList : Control
    {
        public static readonly DependencyProperty CompletionsProperty =
            DependencyProperty.Register("Completions", typeof(ObservableCollection<ICompletionData>), typeof(CompletionList), new PropertyMetadata(OnCompletionsChanged));

        public ObservableCollection<ICompletionData> Completions
        {
            get { return (ObservableCollection<ICompletionData>)GetValue(CompletionsProperty); }
            set { SetValue(CompletionsProperty, value); }
        }
        /// <summary>
        /// 如果是true，则用String.SubString过滤候选项。
        /// 如果是false，则用String.StartWith，并且不过滤。
        /// </summary>
        public Boolean IsFiltering { get => _isFiltering; set => _isFiltering = value; }

        public CompletionList()
        {
        }

        public void CheckTemplate()
        {
        }

        protected static void OnCompletionsChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            CompletionList list = dp as CompletionList;
            if (e.OldValue != null)
            {
                ObservableCollection<ICompletionData> collection = e.OldValue as ObservableCollection<ICompletionData>;
                collection.CollectionChanged -= list.OnCompletionCollectionChanged;
            }
            if (e.NewValue != null)
            {
                ObservableCollection<ICompletionData> collection = e.NewValue as ObservableCollection<ICompletionData>;
                collection.CollectionChanged += list.OnCompletionCollectionChanged;
            }
            list.OnCompletionCollectionChanged(list.Completions, null);
        }

        private void OnCompletionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            CheckTemplate();
        }

        protected Boolean _isFiltering;
    }
}
