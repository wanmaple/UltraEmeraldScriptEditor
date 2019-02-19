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
        static CompletionWindow()
        {
            WidthProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(200.0));
            HeightProperty.OverrideMetadata(typeof(CompletionWindow), new FrameworkPropertyMetadata(120.0));
        }

        public CompletionWindow() 
            : base()
        {
            _completionList = new CompletionList();
            _completionList.Completions = _allCompletions;
            _allCompletions.Add(new StringCompletion(".equ", ".equ"));
            _allCompletions.Add(new StringCompletion(".word", ".word"));
            _allCompletions.Add(new StringCompletion(".hword", ".hword"));
            _allCompletions.Add(new StringCompletion(".byte", ".byte"));
            _allCompletions.Add(new StringCompletion(".include", ".include"));
            _allCompletions.Add(new StringCompletion(".macro", ".macro"));
            _allCompletions.Add(new StringCompletion(".freespace", ".freespace"));
            _allCompletions.Add(new StringCompletion(".endm", ".endm"));
            _allCompletions.Add(new StringCompletion(".org", ".org"));
            _allCompletions.Add(new StringCompletion(".global", ".global"));
            Content = _completionList;
            _enterCommandPushed = false;
            _selectionEventRegistered = false;

            IsVisibleChanged += OnWindowVisibleChanged;
        }

        private void OnWindowVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_completionList._listBox == null)
            {
                _completionList.ApplyTemplate();
            }
            if (!IsVisible)
            {
                if (_selectionEventRegistered)
                {
                    _completionList._listBox.SelectionChanged -= OnCompletionListSelectionChanged;
                    _selectionEventRegistered = false;
                }
                if (_enterCommandPushed)
                {
                    _editview.PopInputHandler();
                    _enterCommandPushed = false;
                }
            }
            else if (IsVisible && !_selectionEventRegistered)
            {
                _completionList._listBox.SelectionChanged += OnCompletionListSelectionChanged;
                _selectionEventRegistered = true;
            }
        }

        private void OnCompletionListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_completionList._listBox.SelectedIndex >= 0)
            {
                if (!_enterCommandPushed)
                {
                    if (_enterInputHandler == null)
                    {
                        var inputHandler = new InputCommandsHandler(_editview);
                        inputHandler.CommandBindings.Add(new CommandBinding(CodeCompletionCommands.RequestCompletion, new ExecutedRoutedEventHandler(OnEnterInput)));
                        inputHandler.InputBindings.Add(new KeyBinding(CodeCompletionCommands.RequestCompletion, Key.Enter, ModifierKeys.None));
                        _enterInputHandler = inputHandler;
                    }
                    _editview.PushInputHandler(_enterInputHandler);
                    _enterCommandPushed = true;
                }
            }
        }

        private void OnEnterInput(Object sender, ExecutedRoutedEventArgs e)
        {
            ICompletionData completion = _completionList._listBox.SelectedItem as ICompletionData;
            completion.PerformCompletion(_editview, _startOffset, _endOffset);
            e.Handled = true;
        }

        #region Overrides
        public override void RequestCompletion(ICompletionData completion)
        {
            completion.PerformCompletion(_editview, _startOffset, _endOffset);
        }

        public override void Filter(string filterText)
        {
            if (_completionList.IsFiltering)
            {

            }
        }

        public override void SelectPreviousCompletion()
        {
            if (_completionList._listBox != null)
            {
                if (_completionList._listBox.SelectedIndex < 0)
                {
                    _completionList._listBox.SelectedIndex = 0;
                }
                else if (_completionList._listBox.SelectedIndex > 0)
                {
                    --_completionList._listBox.SelectedIndex;
                }
                _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
            }
        }

        public override void SelectNextCompletion()
        {
            if (_completionList._listBox != null)
            {
                if (_completionList._listBox.SelectedIndex < 0)
                {
                    _completionList._listBox.SelectedIndex = 0;
                }
                else if (_completionList._listBox.SelectedIndex < _completionList.Completions.Count - 1)
                {
                    ++_completionList._listBox.SelectedIndex;
                }
                _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
            }
        }
        #endregion

        protected CompletionList _completionList;
        private Boolean _selectionEventRegistered;
        private Boolean _enterCommandPushed;
        private IInputHandler _enterInputHandler;
    }
}
