using EditorSupport.CodeCompletion;
using EditorSupport.Document;
using EditorSupport.Rendering;
using System;
using System.Collections.Generic;
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

namespace EditorSupport
{
    /// <summary>
    /// Interaction logic for TextEditor.xaml
    /// </summary>
    public partial class TextEditor : UserControl
    {
        #region Properties
        public static readonly DependencyProperty DocumentProperty =
    DependencyProperty.Register("Document", typeof(TextDocument), typeof(TextEditor), new PropertyMetadata(new TextDocument(), OnDocumentChanged));
        public static readonly DependencyProperty SyntaxProperty =
            DependencyProperty.Register("Syntax", typeof(String), typeof(TextEditor), new PropertyMetadata("PScript"));
        public static readonly DependencyProperty EditorPaddingProperty =
            DependencyProperty.Register("EditorPadding", typeof(Thickness), typeof(TextEditor), new PropertyMetadata(new Thickness(0)));
        public static readonly DependencyProperty EditorFontFamilyProperty =
            DependencyProperty.Register("EditorFontFamily", typeof(FontFamily), typeof(TextEditor), new PropertyMetadata(new FontFamily("consolas"), OnFontOptionChanged));
        // Using a DependencyProperty as the backing store for FontSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditorFontSizeProperty =
            DependencyProperty.Register("EditorFontSize", typeof(Int32), typeof(TextEditor), new PropertyMetadata(15, OnFontOptionChanged));
        public static readonly DependencyProperty CodeCompletionWindowProperty =
            DependencyProperty.Register("CodeCompletionWindow", typeof(CompletionWindow), typeof(TextEditor), new PropertyMetadata(OnCompletionWindowChanged));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public String Syntax
        {
            get { return (String)GetValue(SyntaxProperty); }
            set { SetValue(SyntaxProperty, value); }
        }
        public Thickness EditorPadding
        {
            get { return (Thickness)GetValue(EditorPaddingProperty); }
            set { SetValue(EditorPaddingProperty, value); }
        }
        public FontFamily EditorFontFamily
        {
            get { return (FontFamily)GetValue(EditorFontFamilyProperty); }
            set { SetValue(EditorFontFamilyProperty, value); }
        }
        public Int32 EditorFontSize
        {
            get { return (Int32)GetValue(EditorFontSizeProperty); }
            set { SetValue(EditorFontSizeProperty, value); }
        }
        public CompletionWindow CodeCompletionWindow
        {
            get { return (CompletionWindow)GetValue(CodeCompletionWindowProperty); }
            set { SetValue(CodeCompletionWindowProperty, value); }
        }
        #endregion

        #region Constructor
        public TextEditor()
        {
            InitializeComponent();
        }
        #endregion

        #region Overrides
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (e.Text == ".")
            {
                CodeCompletionWindow.Display();
            }
        }
        #endregion

        #region PropertyChange EventHandlers
        private static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            TextEditor editor = dp as TextEditor;
            editor.OnDocumentChanged(e.OldValue as TextDocument, e.NewValue as TextDocument);
        }
        private void OnDocumentChanged(TextDocument oldDoc, TextDocument newDoc)
        {
            editview.Document = newDoc;
            editview.Focus();
        }

        private static void OnFontOptionChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            TextEditor editor = dp as TextEditor;
            editor.OnFontOptionChanged();
        }
        private void OnFontOptionChanged()
        {
            renderview.GlyphOption.FontFamily = EditorFontFamily;
            renderview.GlyphOption.FontSize = EditorFontSize;
        }

        private static void OnCompletionWindowChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            TextEditor editor = dp as TextEditor;
            editor.OnCompletionWindowChanged();
        }
        private void OnCompletionWindowChanged()
        {
            if (CodeCompletionWindow != null)
            {
                CodeCompletionWindow.EditView = editview;
            }
        }
        #endregion
    }
}
