using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace EditorSupport.CodeCompletion
{
    public delegate void CodeCompletionRoutedEventHandler(Object sender, CodeCompletionRoutedEventArgs e);

    public sealed class CodeCompletionRoutedEventArgs : RoutedEventArgs
    {
        public Boolean ShowCompletion
        {
            get => _showCompletion;
            set => _showCompletion = value;
        }
        public Action<CompletionWindowBase> CompletionWindowHandler
        {
            get => _completionWinHandler;
            set => _completionWinHandler = value;
        }
        public TextCompositionEventArgs InputArgs => _inputArgs;

        public CodeCompletionRoutedEventArgs(RoutedEvent routedEvent, TextCompositionEventArgs inputArgs)
            : base(routedEvent)
        {
            _showCompletion = false;
            _completionWinHandler = null;
            _inputArgs = inputArgs ?? throw new ArgumentNullException("inputArgs");
        }

        private Boolean _showCompletion;
        private Action<CompletionWindowBase> _completionWinHandler;
        private TextCompositionEventArgs _inputArgs;
    }
}
