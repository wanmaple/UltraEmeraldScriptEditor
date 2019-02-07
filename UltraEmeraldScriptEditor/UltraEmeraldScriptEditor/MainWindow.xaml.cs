using EditorSupport.Document;
using EditorSupport.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        }

        private void NewScript(Object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void OpenScript(Object sender, ExecutedRoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.RestoreDirectory = true;
            ofd.Filter = "PScript Files|*.s";
            if (ofd.ShowDialog().Value)
            {
                String filePath = ofd.FileName;
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        String text = CommonUtilities.NormalizeText(sr);
                        var doc = new TextDocument(text);
                        editor.Document = doc;
                    }
                }
            }
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
