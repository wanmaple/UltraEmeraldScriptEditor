using EditorSupport.CodeCompletion;
using EditorSupport.Document;
using EditorSupport.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            editor.Document = new TextDocument();
            editor.Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // 需要加上这行代码，不然进程无法完全杀掉
            editor.CodeCompletionWindow.Close();
        }

        #region Command handlers
        private void NewScript(Object sender, ExecutedRoutedEventArgs e)
        {
            var doc = new TextDocument();
            AddNewDocument(doc);
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
                        editor.Focus();
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

        private void CompletionRequesting(Object sender, CodeCompletionRoutedEventArgs e)
        {
            if (e.InputArgs.Text == ".")
            {
                e.ShowCompletion = true;
                e.CompletionWindowHandler = new Action<CompletionWindowBase>(ResetKeywordsCodeCompletion);
            }
            e.Handled = true;
        }
        #endregion

        private void ResetKeywordsCodeCompletion(CompletionWindowBase completionWindow)
        {
            completionWindow.Completions.Clear();
            completionWindow.Completions.Add(new StringCompletion(".equ", ".equ description"));
            completionWindow.Completions.Add(new StringCompletion(".word", ".word description"));
            completionWindow.Completions.Add(new StringCompletion(".hword", ".hword description"));
            completionWindow.Completions.Add(new StringCompletion(".byte", ".byte description"));
            completionWindow.Completions.Add(new StringCompletion(".string", ".string description"));
            completionWindow.Completions.Add(new StringCompletion(".include", ".include description"));
            completionWindow.Completions.Add(new StringCompletion(".macro", ".macro description"));
            completionWindow.Completions.Add(new StringCompletion(".freespace", ".freespace description"));
            completionWindow.Completions.Add(new StringCompletion(".endm", ".endm description"));
            completionWindow.Completions.Add(new StringCompletion(".org", ".org description"));
            completionWindow.Completions.Add(new StringCompletion(".global", ".global description"));
        }

        private void AddNewDocument(TextDocument doc)
        {
            editor.Document = doc;
            editor.Focus();
        }
    }
}
