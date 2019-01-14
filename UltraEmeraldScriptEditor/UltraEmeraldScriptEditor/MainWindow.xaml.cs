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

            for (int i = 0; i < 1; i++)
            {
                var list = new List<Int32>();
                var rd = new Random();
                Int32 size = 10000;
                for (int j = 0; j < size; j++)
                {
                    list.Add(rd.Next(0, size));
                }
                //list = new List<Int32> {
                //    8, 8, 7, 5, 3, 0, 0, 6, 2, 7,
                //};
                DataStructure.RedBlackTree<Int32> bstree = null;
                try
                {
                    bstree = new DataStructure.RedBlackTree<Int32>(list);
                    //bstree.Add(20);
                    //var oldlist = new List<Int32>();
                    //foreach (var item in bstree)
                    //{
                    //    oldlist.Add(item);
                    //}
                    var elem = list[rd.Next(0, list.Count - 1)];
                    //elem = 7;
                    bstree.Remove(elem);
                    //var newlist = new List<Int32>();
                    //foreach (var item in bstree)
                    //{
                    //    newlist.Add(item);
                    //}
                    bstree.CheckValid();

                    var sb = new StringBuilder();
                    foreach (var item in bstree)
                    {
                        sb.Append(String.Format("{0} ", item.ToString()));
                    }
                    lbTest.Text = sb.ToString();
                    //lbTest.Text = bstree.Draw();
                }
                catch (System.IO.IOException)
                {
                    lbTest.Text = bstree.Draw();
                    break;
                }
            }
        }
    }
}
