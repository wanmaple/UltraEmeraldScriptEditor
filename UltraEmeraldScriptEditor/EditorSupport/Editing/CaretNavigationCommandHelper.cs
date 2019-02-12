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
            AddCommandBinding(EditingCommands.MoveLeftByCharacter, ModifierKeys.None, Key.Left, SelectionHandler(CaretMovementType.CharacterLeft, false));
            AddCommandBinding(EditingCommands.MoveRightByCharacter, ModifierKeys.None, Key.Right, SelectionHandler(CaretMovementType.CharacterRight, false));
            AddCommandBinding(EditingCommands.SelectLeftByCharacter, ModifierKeys.Shift, Key.Left, SelectionHandler(CaretMovementType.CharacterLeft, true));
            AddCommandBinding(EditingCommands.SelectRightByCharacter, ModifierKeys.Shift, Key.Right, SelectionHandler(CaretMovementType.CharacterRight, true));
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

        private static ExecutedRoutedEventHandler SelectionHandler(CaretMovementType caretMovementType, Boolean doSelect)
        {
            return (sender, e) =>
            {
                EditView editor = sender as EditView;
                editor.MoveCaret(caretMovementType, doSelect);
                editor.Redraw();
            };
        }

        internal static List<CommandBinding> _commandBindings = new List<CommandBinding>();
        internal static List<InputBinding> _inputBindings = new List<InputBinding>();
    }
}
