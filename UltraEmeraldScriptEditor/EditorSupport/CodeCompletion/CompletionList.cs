using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EditorSupport.CodeCompletion
{
    public class CompletionList : Control
    {
        public static readonly DependencyProperty CompletionsProperty =
            DependencyProperty.Register("Completions", typeof(AutoFilterObservableCollection<ICompletionData>), typeof(CompletionList), new PropertyMetadata(OnCompletionsChanged));

        public AutoFilterObservableCollection<ICompletionData> Completions
        {
            get { return (AutoFilterObservableCollection<ICompletionData>)GetValue(CompletionsProperty); }
            set { SetValue(CompletionsProperty, value); }
        }
        /// <summary>
        /// 如果是true，则用String.SubString过滤候选项。
        /// 如果是false，则用String.StartWith，并且不过滤。
        /// </summary>
        public Boolean IsFiltering { get => _isFiltering; set => _isFiltering = value; }
        public Int32 VisibleChildrenCount
        {
            get
            {
                if (Completions.Count == 0)
                {
                    return 0;
                }
                if (_listBox == null)
                {
                    ApplyTemplate();
                }
                ScrollViewer scrollViewer = null;
                var border = VisualTreeHelper.GetChild(_listBox, 0) as Border;
                if (border != null)
                {
                    scrollViewer = border.Child as ScrollViewer;
                }
                if (scrollViewer == null || scrollViewer.ExtentHeight == 0.0)
                {
                    return 5;
                }
                Int32 ceiling = (Int32)Math.Ceiling(_listBox.Items.Count * scrollViewer.ViewportHeight / scrollViewer.ExtentHeight);
                return ceiling;
            }
        }

        static CompletionList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CompletionList), new FrameworkPropertyMetadata(typeof(CompletionList)));
        }

        public CompletionList()
            : base()
        {
            _isFiltering = false;
        }

        public void Filter(String text)
        {
            if (String.IsNullOrEmpty(text))
            {
                Completions.Filter(null, null);
                _listBox.SelectedIndex = -1;
                return;
            }
            if (_isFiltering)
            {
                Completions.Filter(data =>
                {
                    Int32 index = data.Text.IndexOf(text);
                    if (index < 0)
                    {
                        data.Priority = Int32.MinValue;
                        return false;
                    }
                    data.Priority = Int32.MaxValue - index;
                    return true;
                }, data => data.Priority);
            }
            else
            {
                Completions.Filter(data =>
                {
                    Boolean match = true;
                    Int32 matchLength = 0;
                    if (text.Length > data.Text.Length)
                    {
                        data.Priority = Int32.MinValue;
                        return false;
                    }
                    for (int i = 0; i < text.Length; i++)
                    {
                        if (text[i] == data.Text[i])
                        {
                            ++matchLength;
                        }
                        else
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        data.Priority = matchLength;
                    }
                    else
                    {
                        data.Priority = Int32.MinValue;
                    }
                    return match;
                }, data => data.Priority);
                _listBox.SelectedIndex = 0;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = GetTemplateChild("TemplateListBox") as ListBox;
            _listBox.ItemsSource = Completions;
            _emptyRegion = GetTemplateChild("TemplateEmptyRegion") as Grid;
            _scrollViewer = null;
            CheckCompletionsEmpty();
        }

        protected static void OnCompletionsChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            CompletionList list = dp as CompletionList;
            if (e.OldValue != null)
            {
                AutoFilterObservableCollection<ICompletionData> collection = e.OldValue as AutoFilterObservableCollection<ICompletionData>;
                collection.CollectionChanged -= list.OnCompletionCollectionChanged;
            }
            if (e.NewValue != null)
            {
                AutoFilterObservableCollection<ICompletionData> collection = e.NewValue as AutoFilterObservableCollection<ICompletionData>;
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
        private ScrollViewer _scrollViewer;
    }
}
