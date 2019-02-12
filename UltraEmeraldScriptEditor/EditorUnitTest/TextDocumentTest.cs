using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using EditorSupport.Document;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EditorUnitTest
{
    [TestClass]
    public class TextDocumentTest
    {
        [TestMethod]
        public void TestCreation()
        {
            CreateDocument();
        }

        [TestMethod]
        public void TestInsertion()
        {
            var doc = CreateDocument();
            doc.Insert(0, "FFFFF");
            doc.Insert(26, "\r\nI'm Maple Wan.\r\n");
            doc.Insert(doc.Length, "000000");
            doc.Insert(doc.Length, "\r\n終わりだな。");
        }

        [TestMethod]
        public void TestALotOfInsertions()
        {
            var doc = new TextDocument();
            var rd = new Random();
            for (int i = 0; i < 100000; i++)
            {
                Int32 offset = rd.Next(0, doc.Length);
                doc.Insert(offset, "12345\n67890\n");
            }
        }

        [TestMethod]
        public void TestRemoval()
        {
            var doc = CreateDocument();
            doc.Remove(16, 12);
            doc.Remove(3, doc.Length - 3);
        }

        [TestMethod]
        public void TestAnchor()
        {
            var doc = CreateDocument();
            var anchors = new List<TextAnchor>();
            for (int i = 0; i < 4; i++)
            {
                anchors.Add(doc.CreateAnchor((i + 1) * 5));
            }
            var anchor = anchors.Last();
            doc.MoveAnchorLeft(anchor, 1);
            doc.MoveAnchorLeft(anchor, 2);
            doc.MoveAnchorLeft(anchor, 5);
            doc.MoveAnchorLeft(anchor, 10);
            doc.MoveAnchorRight(anchor, 10);
            doc.MoveAnchorRight(anchor, 5);
            doc.MoveAnchorRight(anchor, 2);
            doc.MoveAnchorRight(anchor, 1);
        }

        private TextDocument CreateDocument()
        {
            var sb = new StringBuilder();
            sb.AppendLine("abcdefghijklmn");
            sb.AppendLine("1234567890");
            sb.AppendLine("I am a cheater.");
            sb.AppendLine("这是一段中文。");
            var doc = new TextDocument(sb.ToString());
            return doc;
        }
    }
}
