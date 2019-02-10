using EditorSupport.Document;
using EditorSupport.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace EditorSupport.Editing
{
    public interface IEditInfo
    {
        /// <summary>
        /// 改变文本对象
        /// </summary>
        /// <param name="doc"></param>
        void ChangeDocument(TextDocument doc);
        /// <summary>
        /// 测量光标大小、绘制位置等信息
        /// </summary>
        /// <param name="caret"></param>
        void MeasureCaretRendering(Caret caret);
        /// <summary>
        /// 测量光标文本位置
        /// </summary>
        /// <param name="caret"></param>
        void MeasureCaretLocation(Caret caret, Point positionToView);
    }
}
