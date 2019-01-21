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
using UltraEmeraldScriptEditor.Mvvm;

namespace UltraEmeraldScriptEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            EditorSupport.Document.Rope<Char> rope = new EditorSupport.Document.Rope<char>();
            String test = "1234567890abcdefghijk";
            rope.InsertRange(0, test.ToArray());
            rope.AddRange("lmnopqrstuvwxyz".ToArray());
            System.Diagnostics.Debug.WriteLine(rope[3]);
            System.Diagnostics.Debug.WriteLine(rope[15]);
            rope.RemoveRange(0, rope.Count);
            foreach (var item in rope)
            {
                System.Diagnostics.Debug.WriteLine(item);
            }
        }

        private void NewScript(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OpenScript(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void SaveScript(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void SaveScriptAs(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void QuitApplication(Object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
            e.Handled = true;
        }

        private void OpenSettings(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
