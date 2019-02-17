using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorSupport.CodeCompletion
{
    /// <summary>
    /// Interaction logic for CompletionList.xaml
    /// </summary>
    public partial class CompletionList : UserControl
    {
        public static readonly DependencyProperty CompletionsProperty =
            DependencyProperty.Register("Completions", typeof(ObservableCollection<String>), typeof(CompletionList), new PropertyMetadata());
        public static readonly DependencyProperty EmptyTemplateProperty =
            DependencyProperty.Register("EmptyTemplate", typeof(ControlTemplate), typeof(CompletionList), new PropertyMetadata());

        public ObservableCollection<ICompletionData> Completions
        {
            get { return (ObservableCollection<ICompletionData>)GetValue(CompletionsProperty); }
            set { SetValue(CompletionsProperty, value); }
        }
        /// <summary>
        /// 当列表里没有选项时会显示这个模板
        /// </summary>
        public ControlTemplate EmptyTemplate
        {
            get { return (ControlTemplate)GetValue(EmptyTemplateProperty); }
            set { SetValue(EmptyTemplateProperty, value); }
        }
        /// <summary>
        /// 如果是true，则用String.SubString过滤候选项。
        /// 如果是false，则用String.StartWith，并且不过滤。
        /// </summary>
        public Boolean IsFiltering { get => _isFiltering; set => _isFiltering = value; }

        public CompletionList()
        {
            InitializeComponent();
        }
        
        protected Boolean _isFiltering;
    }
}
