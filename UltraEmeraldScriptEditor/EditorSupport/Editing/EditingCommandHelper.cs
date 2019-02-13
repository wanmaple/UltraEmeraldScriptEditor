using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace EditorSupport.Editing
{
    internal static class EditingCommandHelper
    {
        internal static IInputHandler CreateHandler(EditView owner)
        {
            var handler = new InputCommandsHandler(owner);
            foreach (CommandBinding binding in _commandBindings)
            {
                handler.CommandBindings.Add(binding);
            }
            foreach (InputBinding binding in _inputBindings)
            {
                handler.InputBindings.Add(binding);
            }
            return handler;
        }

        static EditingCommandHelper()
        {
            AddCommandBinding(EditingCommands.EnterParagraphBreak, ModifierKeys.None, Key.Enter, OnEnter);
            AddCommandBinding(EditingCommands.EnterLineBreak, ModifierKeys.Shift, Key.Enter, OnEnter);
            AddCommandBinding(EditingCommands.Backspace, ModifierKeys.None, Key.Back, RemoveHandler(EditingCommands.SelectLeftByCharacter));
            AddCommandBinding(EditingCommands.Delete, ModifierKeys.None, Key.Delete, RemoveHandler(EditingCommands.SelectRightByCharacter));
            AddCommandBinding(EditingCommands.TabForward, ModifierKeys.None, Key.Tab, OnTabForward);
            AddCommandBinding(EditingCommands.TabBackward, ModifierKeys.Shift, Key.Tab, OnTabBackward);
        }

        internal static void AddCommandBinding(ICommand command, ModifierKeys modifiers, Key key, ExecutedRoutedEventHandler handler)
        {
            _commandBindings.Add(new CommandBinding(command, handler));
            _inputBindings.Add(CreateFrozenKeyBinding(command, modifiers, key));
        }

        internal static KeyBinding CreateFrozenKeyBinding(ICommand command, ModifierKeys modifiers, Key key)
        {
            var kb = new KeyBinding(command, key, modifiers);
            kb.Freeze();
            return kb;
        }

        /// <summary>
        /// 统一处理成删除selection，command是一个selection相关的命令。
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        private static ExecutedRoutedEventHandler RemoveHandler(RoutedUICommand command)
        {
            return (sender, e) =>
            {
                EditView editor = sender as EditView;
                if (editor != null && editor.Document != null)
                {
                    if (editor.Selection.IsEmpty)
                    {
                        command.Execute(e.Parameter, editor);
                    }
                    editor.RemoveSelection();
                    editor.Redraw();
                }
                e.Handled = true;
            };
        }

        private static void OnEnter(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.InsertText(CommonUtilities.LineBreak);
                editor.Caret.RestartAnimation();
                editor.Redraw();
            }
        }

        private static void OnTabForward(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.TabForward();
                editor.Caret.RestartAnimation();
                editor.Redraw();
            }
        }

        private static void OnTabBackward(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.TabBackward();
                editor.Caret.RestartAnimation();
                editor.Redraw();
            }
        }

        internal static List<CommandBinding> _commandBindings = new List<CommandBinding>();
        internal static List<InputBinding> _inputBindings = new List<InputBinding>();
    }
}
