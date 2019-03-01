using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using EditorSupport.Document;
using EditorSupport.Editing;

namespace EditorSupport.CodeCompletion
{
    public sealed class StringCompletion : ICompletionData
    {
        public StringCompletion(String content, String description, ImageSource image = null)
        {
            _content = _text = content;
            _description = description;
            _image = image;
            _priority = 0;
        }

        #region ICompletionData
        public ImageSource Image
        {
            get => _image;
            set => _image = value;
        }

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public object Content
        {
            get => _content;
            set => _content = value;
        }

        public object Description
        {
            get => _description;
            set => _description = value;
        }

        public int Priority
        {
            get => _priority;
            set => _priority = value;
        }

        public void PerformCompletion(EditView editview, int startOffset, int endOffset)
        {
            // 合并操作
            using (editview.Document.AutoUpdate())
            {
                editview.Caret.DocumentOffset = startOffset;
                editview.Selection.Set(startOffset, endOffset);
                editview.InsertText(Text);
                editview.Redraw();
            }
        }
        #endregion

        private ImageSource _image;
        private String _text;
        private Object _content;
        private Object _description;
        private Int32 _priority;
    }
}
