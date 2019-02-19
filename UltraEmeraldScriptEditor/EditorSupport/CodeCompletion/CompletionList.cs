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

        static CompletionList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompletionList), new FrameworkPropertyMetadata(typeof(CompletionList)));
        }

        public CompletionList()
            : base()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = GetTemplateChild("TemplateListBox") as ListBox;
            _listBox.ItemsSource = Completions;
            _emptyRegion = GetTemplateChild("TemplateEmptyRegion") as Grid;
            CheckCompletionsEmpty();
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
            if (_listBox == null || _emptyRegion == null)
            {
                ApplyTemplate();
            }
            CheckCompletionsEmpty();
        }

        private void CheckCompletionsEmpty()
        {
            if (_listBox == null || _emptyRegion == null)
            {
                return;
            }
            if (Completions.Count > 0)
            {
                _listBox.Visibility = Visibility.Visible;
                _emptyRegion.Visibility = Visibility.Hidden;
            }
            else
            {
                _listBox.Visibility = Visibility.Hidden;
                _emptyRegion.Visibility = Visibility.Visible;
            }
        }

        protected Boolean _isFiltering;
        private Grid _emptyRegion;
        internal ListBox _listBox;
    }
}
