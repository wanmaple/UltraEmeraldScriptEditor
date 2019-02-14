using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Input;

namespace EditorSupport.Editing
{
    internal enum CaretMovementType : Byte
    {
        CharacterLeft,
        CharacterRight,
        WordLeft,
        WordRight,
        LineStart,
        LineEnd,
        LineUp,
        LineDown,
        PageUp,
        PageDown,
        DocumentStart,
        DocumentEnd,
    }

    internal static class CaretNavigationCommandHelper
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

        static CaretNavigationCommandHelper()
        {
            AddCommandBinding(EditingCommands.MoveLeftByCharacter, ModifierKeys.None, Key.Left, CaretHandler(CaretMovementType.CharacterLeft, false));
            AddCommandBinding(EditingCommands.MoveRightByCharacter, ModifierKeys.None, Key.Right, CaretHandler(CaretMovementType.CharacterRight, false));
            AddCommandBinding(EditingCommands.MoveUpByLine, ModifierKeys.None, Key.Up, CaretHandler(CaretMovementType.LineUp, false));
            AddCommandBinding(EditingCommands.MoveDownByLine, ModifierKeys.None, Key.Down, CaretHandler(CaretMovementType.LineDown, false));
            AddCommandBinding(EditingCommands.MoveToLineStart, ModifierKeys.None, Key.Home, CaretHandler(CaretMovementType.LineStart, false));
            AddCommandBinding(EditingCommands.MoveToLineEnd, ModifierKeys.None, Key.End, CaretHandler(CaretMovementType.LineEnd, false));
            AddCommandBinding(EditingCommands.MoveToDocumentStart, ModifierKeys.Control, Key.Home, CaretHandler(CaretMovementType.DocumentStart, false));
            AddCommandBinding(EditingCommands.MoveToDocumentEnd, ModifierKeys.Control, Key.End, CaretHandler(CaretMovementType.DocumentEnd, false));
            AddCommandBinding(EditingCommands.MoveUpByPage, ModifierKeys.None, Key.PageUp, CaretHandler(CaretMovementType.PageUp, false));
            AddCommandBinding(EditingCommands.MoveDownByPage, ModifierKeys.None, Key.PageDown, CaretHandler(CaretMovementType.PageDown, false));

            AddCommandBinding(EditingCommands.SelectLeftByCharacter, ModifierKeys.Shift, Key.Left, CaretHandler(CaretMovementType.CharacterLeft, true));
            AddCommandBinding(EditingCommands.SelectRightByCharacter, ModifierKeys.Shift, Key.Right, CaretHandler(CaretMovementType.CharacterRight, true));
            AddCommandBinding(EditingCommands.SelectUpByLine, ModifierKeys.Shift, Key.Up, CaretHandler(CaretMovementType.LineUp, true));
            AddCommandBinding(EditingCommands.SelectDownByLine, ModifierKeys.Shift, Key.Down, CaretHandler(CaretMovementType.LineDown, true));
            AddCommandBinding(EditingCommands.SelectToLineStart, ModifierKeys.Shift, Key.Home, CaretHandler(CaretMovementType.LineStart, true));
            AddCommandBinding(EditingCommands.SelectToLineEnd, ModifierKeys.Shift, Key.End, CaretHandler(CaretMovementType.LineEnd, true));
            AddCommandBinding(EditingCommands.SelectToDocumentStart, ModifierKeys.Control | ModifierKeys.Shift, Key.Home, CaretHandler(CaretMovementType.DocumentStart, true));
            AddCommandBinding(EditingCommands.SelectToDocumentEnd, ModifierKeys.Control | ModifierKeys.Shift, Key.End, CaretHandler(CaretMovementType.DocumentEnd, true));
            AddCommandBinding(EditingCommands.SelectUpByPage, ModifierKeys.Shift, Key.PageUp, CaretHandler(CaretMovementType.PageUp, true));
            AddCommandBinding(EditingCommands.SelectDownByPage, ModifierKeys.Shift, Key.PageDown, CaretHandler(CaretMovementType.PageDown, true));
            AddCommandBinding(ApplicationCommands.SelectAll, ModifierKeys.Control, Key.A, OnSelectAll);
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

        private static ExecutedRoutedEventHandler CaretHandler(CaretMovementType caretMovementType, Boolean doSelect)
        {
            return (sender, e) =>
            {
                EditView editor = sender as EditView;
                editor.MoveCaret(caretMovementType, doSelect);
                editor.Redraw();
                e.Handled = true;
            };
        }

        private static void OnSelectAll(Object sender, ExecutedRoutedEventArgs e)
        {
            EditView editor = sender as EditView;
            editor.SelectAll();
            editor.Redraw();
            e.Handled = true;
        }

        internal static List<CommandBinding> _commandBindings = new List<CommandBinding>();
        internal static List<InputBinding> _inputBindings = new List<InputBinding>();
    }
}
