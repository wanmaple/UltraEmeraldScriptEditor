using EditorSupport.Document;
using EditorSupport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
            AddCommandBinding(ApplicationCommands.Copy, ModifierKeys.Control, Key.C, OnCopy, CanEdit);
            AddCommandBinding(ApplicationCommands.Cut, ModifierKeys.Control, Key.X, OnCut, CanEdit);
            AddCommandBinding(ApplicationCommands.Paste, ModifierKeys.Control, Key.V, OnPaste, CanEdit);
            AddCommandBinding(ApplicationCommands.Undo, ModifierKeys.Control, Key.Z, OnUndo, CanUndo);
            AddCommandBinding(ApplicationCommands.Redo, ModifierKeys.Control, Key.Y, OnRedo, CanRedo);
        }

        internal static void AddCommandBinding(ICommand command, ModifierKeys modifiers, Key key, ExecutedRoutedEventHandler handler, CanExecuteRoutedEventHandler canExecuteHandler = null)
        {
            _commandBindings.Add(new CommandBinding(command, handler, canExecuteHandler));
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
                    editor.BeginUpdating();
                    if (editor.Selection.IsEmpty)
                    {
                        command.Execute(e.Parameter, editor);
                    }
                    editor.RemoveSelection();
                    editor.EndUpdating();
                    editor.Redraw();
                    e.Handled = true;
                }
            };
        }

        private static void OnEnter(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.BeginUpdating();
                editor.InsertText(CommonUtilities.LineBreak);
                editor.EndUpdating();
                editor.Caret.RestartAnimation();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void CanEdit(Object sender, CanExecuteRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null && editor.IsFocused)
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private static void CanUndo(Object sender, CanExecuteRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null && editor.IsFocused)
            {
                e.CanExecute = editor.CanUndo() && editor.Document.CanUndo();
                e.Handled = true;
            }
        }

        private static void CanRedo(Object sender, CanExecuteRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null && editor.IsFocused)
            {
                e.CanExecute = editor.CanRedo() && editor.Document.CanRedo();
                e.Handled = true;
            }
        }

        private static void OnCopy(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                String targetText = String.Empty;
                if (editor.Selection.IsEmpty)
                {
                    // 拷贝整行
                    CopyWholeLine(editor);
                }
                else
                {
                    // 拷贝选中的文字
                    CopySelectedText(editor);
                }
                e.Handled = true;
            }
        }

        private static void OnCut(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                String targetText = String.Empty;
                if (editor.Selection.IsEmpty)
                {
                    // 剪切整行
                    CopyWholeLine(editor);
                    editor.SelectLine();
                }
                else
                {
                    // 剪切选中的文字
                    CopySelectedText(editor);
                }
                editor.BeginUpdating();
                editor.RemoveSelection();
                editor.EndUpdating();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void OnPaste(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                IDataObject obj = null;
                try
                {
                    obj = Clipboard.GetDataObject();
                }
                catch (ExternalException)
                {
                    return;
                }
                if (obj == null)
                {
                    return;
                }
                String text = (String)obj.GetData(DataFormats.UnicodeText);
                text = CommonUtilities.NormalizeText(new StringReader(text));
                if (!String.IsNullOrEmpty(text))
                {
                    editor.BeginUpdating();
                    if (obj.GetDataPresent(LineCopyFormat))
                    {
                        // 粘贴整行
                        editor.InsertLine(text);
                        editor.Redraw();
                    }
                    else
                    {
                        editor.InsertText(text);
                        editor.Redraw();
                    }
                    editor.EndUpdating();
                }
                e.Handled = true;
            }
        }

        private static void OnUndo(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.Undo();
                editor.MoveCaretInVisual();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void OnRedo(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.Redo();
                editor.MoveCaretInVisual();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void OnTabForward(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.BeginUpdating();
                editor.TabForward();
                editor.EndUpdating();
                editor.Caret.RestartAnimation();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void OnTabBackward(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            if (editor != null && editor.Document != null)
            {
                editor.BeginUpdating();
                editor.TabBackward();
                editor.EndUpdating();
                editor.Caret.RestartAnimation();
                editor.Redraw();
                e.Handled = true;
            }
        }

        private static void CopyWholeLine(EditView editor)
        {
            DocumentLine line = editor.Document.GetLineByOffset(editor.Caret.DocumentOffset);
            String lineText = editor.Document.GetTextAt(line.StartOffset, line._exactLength);
            var obj = new DataObject(lineText);
            MemoryStream ms = new MemoryStream(1);
            ms.WriteByte(1);
            obj.SetData(LineCopyFormat, ms, false);
            try
            {
                Clipboard.SetDataObject(obj, true);
            }
            catch (ExternalException)
            {
                // 有时候会莫名其妙抛出这个异常，微软的控件都是忽略这个异常，所以这里也做同样的事
                return;
            }
        }

        private static void CopySelectedText(EditView editor)
        {
            String text = editor.Document.GetTextAt(editor.Selection.StartOffset, editor.Selection.Length);
            var obj = new DataObject(text);
            try
            {
                Clipboard.SetDataObject(obj, true);
            }
            catch (ExternalException)
            {
                // 有时候会莫名其妙抛出这个异常，微软的控件都是忽略这个异常，所以这里也做同样的事
                return;
            }
        }

        private static readonly String LineCopyFormat = "MSDEVLineSelect";

        internal static List<CommandBinding> _commandBindings = new List<CommandBinding>();
        internal static List<InputBinding> _inputBindings = new List<InputBinding>();
    }
}
