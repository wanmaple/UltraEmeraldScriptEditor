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

            Int32 len = 10000;
            Char[] chars = new char[len];
            var rd = new Random();
            for (int i = 0; i < len; i++)
            {
                chars[i] = (Char)rd.Next(97, 122);
            }
            var doc = new EditorSupport.Document.TextDocument(chars);
            var anchors = new List<EditorSupport.Document.TextAnchor>();
            for (int i = 0; i < 20; i++)
            {
                Int32 offset = rd.Next(0, len);
                anchors.Add(doc.CreateAnchor(offset));
            }
            foreach (var anchor in anchors)
            {
                doc.RemoveAnchor(anchor);
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
