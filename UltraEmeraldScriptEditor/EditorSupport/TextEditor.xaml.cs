﻿using EditorSupport.CodeCompletion;
using EditorSupport.Document;
using EditorSupport.Editing;
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
            DependencyProperty.Register("CodeCompletionWindow", typeof(CompletionWindowBase), typeof(TextEditor), new PropertyMetadata(OnCompletionWindowChanged));

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
        public CompletionWindowBase CodeCompletionWindow
        {
            get { return (CompletionWindowBase)GetValue(CodeCompletionWindowProperty); }
            set { SetValue(CodeCompletionWindowProperty, value); }
        }
        #endregion

        #region Constructor
        public TextEditor()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }
        #endregion

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (CodeCompletionWindow == null || !CodeCompletionWindow.IsVisible)
            {
                return;
            }

            if (e.Key == Key.Back)
            {
                if (CodeCompletionWindow.EndOffset > CodeCompletionWindow.StartOffset)
                {
                    --CodeCompletionWindow.EndOffset;
                }
            }
            else if (e.Key == Key.Space)
            {
                CodeCompletionWindow.Collapse();
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (CodeCompletionWindow != null)
            {
                CodeCompletionWindow.Collapse();
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (CodeCompletionWindow != null)
            {
                CodeCompletionWindow.Collapse();
            }
        }

        private IInputHandler CreateCodeCompletionInputHandler()
        {
            var inputHandler = new InputCommandsHandler(editview);
            AddCommandBinding(inputHandler, CodeCompletionCommands.SelectPreviousCompletion, ModifierKeys.None, Key.Up, new ExecutedRoutedEventHandler(SelectPreviousCompletion), new CanExecuteRoutedEventHandler(CanOperate));
            AddCommandBinding(inputHandler, CodeCompletionCommands.SelectNextCompletion, ModifierKeys.None, Key.Down, new ExecutedRoutedEventHandler(SelectNextCompletion), new CanExecuteRoutedEventHandler(CanOperate));
            return inputHandler;
        }

        private void SelectPreviousCompletion(Object sender, ExecutedRoutedEventArgs e)
        {
            if (CodeCompletionWindow != null)
            {
                CodeCompletionWindow.SelectPreviousCompletion();
            }
        }

        private void SelectNextCompletion(Object sender, ExecutedRoutedEventArgs e)
        {
            if (CodeCompletionWindow != null)
            {
                CodeCompletionWindow.SelectNextCompletion();
            }
        }

        private void CanOperate(Object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CodeCompletionWindow != null && CodeCompletionWindow.IsVisible;
        }

        private void AddCommandBinding(InputCommandsHandler commandsHandler, ICommand command, ModifierKeys modifiers, Key key, ExecutedRoutedEventHandler executeHandler, CanExecuteRoutedEventHandler canExecuteHandler = null)
        {
            commandsHandler.CommandBindings.Add(new CommandBinding(command, executeHandler, canExecuteHandler));
            commandsHandler.InputBindings.Add(CreateFrozenKeyBinding(command, modifiers, key));
        }

        private KeyBinding CreateFrozenKeyBinding(ICommand command, ModifierKeys modifiers, Key key)
        {
            var kb = new KeyBinding(command, key, modifiers);
            kb.Freeze();
            return kb;
        }

        #region Event handlers
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            editview.PreviewTextInput += OnPreviewTextInput;
            editview.Caret.PositionChanged += OnCaretPositionChanged;
        }

        private void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (CodeCompletionWindow == null)
            {
                return;
            }
            if (!CodeCompletionWindow.IsVisible && e.Text == ".")
            {
                CodeCompletionWindow.StartOffset = CodeCompletionWindow.EndOffset = editview.Caret.DocumentOffset;
                CodeCompletionWindow.Display();
            }
            ++CodeCompletionWindow.EndOffset;
        }

        private void OnCaretPositionChanged(object sender, EventArgs e)
        {
            if (CodeCompletionWindow == null || !CodeCompletionWindow.IsVisible)
            {
                return;
            }

            if (editview.Caret.DocumentOffset <= CodeCompletionWindow.StartOffset || editview.Caret.DocumentOffset > CodeCompletionWindow.EndOffset)
            {
                CodeCompletionWindow.Collapse();
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
            editor.OnCompletionWindowChanged(e.OldValue as CompletionWindowBase, e.NewValue as CompletionWindowBase);
        }
        private void OnCompletionWindowChanged(CompletionWindowBase oldWindow, CompletionWindowBase newWindow)
        {
            if (oldWindow != null)
            {
                oldWindow.EditView = null;
                oldWindow.IsVisibleChanged -= OnCompletionWindowVisibleChanged;
            }
            if (newWindow != null)
            {
                newWindow.EditView = editview;
                newWindow.IsVisibleChanged += OnCompletionWindowVisibleChanged;
            }
        }
        private void OnCompletionWindowVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (CodeCompletionWindow.IsVisible)
            {
                editview.PushInputHandler(CreateCodeCompletionInputHandler());
            }
            else
            {
                editview.PopInputHandler();
            }
        }
        #endregion
    }
}
