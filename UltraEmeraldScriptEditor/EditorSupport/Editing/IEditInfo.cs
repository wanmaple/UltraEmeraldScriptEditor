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
        /// 测量选中文字大小、绘制位置等信息
        /// </summary>
        /// <param name="selection"></param>
        void MeasureSelectionRendering(Selection selection);
        /// <summary>
        /// 测量光标文本位置
        /// </summary>
        /// <param name="caret"></param>
        void MeasureCaretLocation(Caret caret, Point positionToView);
        /// <summary>
        /// 计算光标上移的最终偏移
        /// </summary>
        /// <param name="currentOffset"></param>
        /// <returns></returns>
        Int32 LineUpCaretOffset(Int32 currentOffset);
        /// <summary>
        /// 计算光标下移的最终偏移
        /// </summary>
        /// <param name="currentOffset"></param>
        /// <returns></returns>
        Int32 LineDownCaretOffset(Int32 currentOffset);
    }
}
