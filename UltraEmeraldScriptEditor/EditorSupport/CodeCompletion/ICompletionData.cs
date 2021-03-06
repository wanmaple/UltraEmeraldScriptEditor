﻿using EditorSupport.Document;
using EditorSupport.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EditorSupport.CodeCompletion
{
    public interface ICompletionData
    {
        /// <summary>
        /// 显示在左侧的图标
        /// </summary>
        ImageSource Image { get; }
        /// <summary>
        /// 文本，用于过滤
        /// </summary>
        String Text { get; }
        /// <summary>
        /// 表现，可以和Text相同，也可以是其他内容控件
        /// </summary>
        Object Content { get; }
        /// <summary>
        /// 描述，可以是个字符串，也可以是其他内容控件
        /// </summary>
        Object Description { get; }
        /// <summary>
        /// 优先级，优先级高的显示在前面
        /// </summary>
        Int32 Priority { get; set; }
        /// <summary>
        /// 处理自动完成
        /// </summary>
        /// <param name="editview">所在<see cref="EditView"/></param>
        /// <param name="startOffset">表示Completion作用范围的起始偏移</param>
        /// <param name="endOffset">表示Completion作用范围的结束偏移</param>
        void PerformCompletion(EditView editview, Int32 startOffset, Int32 endOffset);
    }
}
