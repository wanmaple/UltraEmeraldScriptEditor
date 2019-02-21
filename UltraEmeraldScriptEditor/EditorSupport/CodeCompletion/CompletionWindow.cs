using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
            _tip = new ToolTip();
            _tip.PlacementTarget = this;
            _tip.Placement = PlacementMode.Right;
            _tip.Closed += OnTipClosed;
            _completionList = new CompletionList();
            _completionList.Completions = new AutoFilterObservableCollection<ICompletionData>(_allCompletions);
            Content = _completionList;
            _enterCommandPushed = false;
            _listBoxEventRegistered = false;

            IsVisibleChanged += OnWindowVisibleChanged;
        }

        private void OnTipClosed(object sender, RoutedEventArgs e)
        {
            if (_tip != null)
            {
                _tip.Content = null;
            }
        }

        private void OnWindowVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_completionList._listBox == null)
            {
                _completionList.ApplyTemplate();
            }
            if (!IsVisible)
            {
                if (_listBoxEventRegistered)
                {
                    _completionList._listBox.SelectionChanged -= OnCompletionListSelectionChanged;
                    _completionList._listBox.PreviewMouseDoubleClick -= OnCompletionListDoubleClick;
                    _listBoxEventRegistered = false;
                }
                if (_enterCommandPushed)
                {
                    _editview.PopInputHandler();
                    _enterCommandPushed = false;
                }
                _tip.IsOpen = false;
            }
            else if (IsVisible)
            {
                if (!_listBoxEventRegistered)
                {
                    _completionList._listBox.SelectionChanged += OnCompletionListSelectionChanged;
                    _completionList._listBox.PreviewMouseDoubleClick += OnCompletionListDoubleClick;
                    _listBoxEventRegistered = true;
                }
                _completionList._listBox.SelectedIndex = -1;
            }
        }

        private void OnCompletionListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (_completionList._listBox.SelectedIndex >= 0)
                {
                    ICompletionData completion = _completionList._listBox.SelectedItem as ICompletionData;
                    completion.PerformCompletion(_editview, _startOffset, _endOffset);
                    e.Handled = true;
                }
            }
            e.Handled = false;
        }

        private void OnCompletionListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_completionList._listBox.SelectedIndex >= 0)
            {
                if (!_enterCommandPushed)
                {
                    if (_completionRequestInputHandler == null)
                    {
                        var inputHandler = new InputCommandsHandler(_editview);
                        var cmdBinding = new CommandBinding(CodeCompletionCommands.RequestCompletion, new ExecutedRoutedEventHandler(OnCompletionRequest));
                        inputHandler.CommandBindings.Add(cmdBinding);
                        inputHandler.InputBindings.Add(new KeyBinding(CodeCompletionCommands.RequestCompletion, Key.Enter, ModifierKeys.None));
                        inputHandler.CommandBindings.Add(cmdBinding);
                        inputHandler.InputBindings.Add(new KeyBinding(CodeCompletionCommands.RequestCompletion, Key.Tab, ModifierKeys.None));
                        _completionRequestInputHandler = inputHandler;
                    }
                    _editview.PushInputHandler(_completionRequestInputHandler);
                    _enterCommandPushed = true;
                }
                ICompletionData completion = _completionList._listBox.SelectedItem as ICompletionData;
                Object description = completion.Description;
                if (description != null)
                {
                    if (description is UIElement)
                    {
                        _tip.Content = description;
                    }
                    else
                    {
                        var textblock = new TextBlock
                        {
                            Text = description.ToString(),
                            TextWrapping = TextWrapping.Wrap,
                        };
                        _tip.Content = textblock;
                    }
                    _tip.IsOpen = true;
                }
            }
            else
            {
                _tip.IsOpen = false;
            }
        }

        private void OnCompletionRequest(Object sender, ExecutedRoutedEventArgs e)
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
            _completionList.Filter(filterText);
        }

        public override void SelectPreviousCompletion()
        {
            if (Completions.Count == 0)
            {
                return;
            }
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
            if (Completions.Count == 0)
            {
                return;
            }
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

        public override void SelectFirstCompletion()
        {
            if (Completions.Count == 0)
            {
                return;
            }
            if (_completionList._listBox != null)
            {
                _completionList._listBox.SelectedIndex = 0;
            }
            _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
        }

        public override void SelectLastCompletion()
        {
            if (Completions.Count == 0)
            {
                return;
            }
            if (_completionList._listBox != null)
            {
                _completionList._listBox.SelectedIndex = _completionList.Completions.Count - 1;
            }
            _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
        }

        public override void SelectPreviousPageCompletion()
        {
            if (Completions.Count == 0)
            {
                return;
            }
            if (_completionList._listBox != null)
            {
                if (_completionList._listBox.SelectedIndex < 0)
                {
                    _completionList._listBox.SelectedIndex = 0;
                }
                else
                {
                    _completionList._listBox.SelectedIndex = Math.Max(_completionList._listBox.SelectedIndex - _completionList.VisibleChildrenCount, 0);
                }
            }
            _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
        }

        public override void SelectNextPageCompletion()
        {
            if (Completions.Count == 0)
            {
                return;
            }
            if (_completionList._listBox != null)
            {
                if (_completionList._listBox.SelectedIndex < 0)
                {
                    _completionList._listBox.SelectedIndex = 0;
                }
                else
                {
                    _completionList._listBox.SelectedIndex = Math.Min(_completionList._listBox.SelectedIndex + _completionList.VisibleChildrenCount, _completionList.Completions.Count - 1);
                }
            }
            _completionList._listBox.ScrollIntoView(_completionList._listBox.SelectedItem);
        }
        #endregion

        protected CompletionList _completionList;
        protected ToolTip _tip;
        private Boolean _listBoxEventRegistered;
        private Boolean _enterCommandPushed;
        private IInputHandler _completionRequestInputHandler;
    }
}
