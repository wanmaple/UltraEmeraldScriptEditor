using EditorSupport.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EditorSupport.CodeCompletion
{
    public static class CodeCompletionCommands
    {
        public static RoutedUICommand SelectPreviousCompletion = new RoutedUICommand("SelectPreviousCompletion", "SelectPreviousCompletion", typeof(EditView));

        public static RoutedUICommand SelectNextCompletion = new RoutedUICommand("SelectNextCompletion", "SelectNextCompletion", typeof(EditView));

        public static RoutedUICommand SelectPreviousPageCompletion = new RoutedUICommand("SelectPreviousPageCompletion", "SelectPreviousPageCompletion", typeof(EditView));

        public static RoutedUICommand SelectNextPageCompletion = new RoutedUICommand("SelectNextPageCompletion", "SelectNextPageCompletion", typeof(EditView));

        public static RoutedUICommand SelectFirstCompletion = new RoutedUICommand("SelectFirstCompletion", "SelectFirstCompletion", typeof(EditView));

        public static RoutedUICommand SelectLastCompletion = new RoutedUICommand("SelectLastCompletion", "SelectLastCompletion", typeof(EditView));

        public static RoutedUICommand RequestCompletion = new RoutedUICommand("RequestCompletion", "RequestCompletion", typeof(EditView));
    }
}
