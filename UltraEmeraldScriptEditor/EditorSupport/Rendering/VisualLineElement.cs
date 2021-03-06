﻿using EditorSupport.Document;
using EditorSupport.Highlighting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// <see cref="VisualLine"/>中的文本元素
    /// </summary>
    public abstract class VisualLineElement : IHighlightee
    {
        #region Abstraction
        /// <summary>
        /// 计算好所有的Drawing，为了能使<see cref="VisualLine"/>将Drawing合并。
        /// </summary>
        /// <param name="drawingContext"></param>
        /// <param name="renderContext"></param>
        public abstract IEnumerable<Drawing> GenerateDrawings(DrawingContext drawingContext, RenderContext renderContext);
        #endregion

        #region Properties
        public VisualLine Owner { get => _owner; }
        /// <summary>
        /// 相对所在行的偏移
        /// </summary>
        public Int32 RelativeOffset { get; set; }
        /// <summary>
        /// 长度
        /// </summary>
        public Int32 Length { get; set; }
        public LineBreakCondition BreakBefore { get; set; }
        public LineBreakCondition BreakAfter { get; set; }
        #endregion

        #region Constructor
        protected VisualLineElement(VisualLine owner, Int32 relativeOffset, Int32 length)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            RelativeOffset = relativeOffset;
            Length = length;
            BreakBefore = LineBreakCondition.BreakPossible;
            BreakAfter = LineBreakCondition.BreakPossible;
            _fgBrush = Brushes.Black;
            _bgBrush = null;
            _fontWeight = FontWeights.Normal;
            _fontStyle = FontStyles.Normal;
            _fontStretch = FontStretches.Normal;
        }
        #endregion

        #region IHighlightee
        public Brush ForegroundBrush { get => _fgBrush; set => _fgBrush = value; }
        public Brush BackgroundBrush { get => _bgBrush; set => _bgBrush = value; }
        public FontWeight FontWeight { get => _fontWeight; set => _fontWeight = value; }
        public FontStyle FontStyle { get => _fontStyle; set => _fontStyle = value; }
        public FontStretch FontStretch { get => _fontStretch; set => _fontStretch = value; }
        public int HighlightRule { get => _highlightRule; set => _highlightRule = value; }
        public abstract string Content { get; }

        protected Brush _fgBrush, _bgBrush;
        protected FontWeight _fontWeight;
        protected FontStyle _fontStyle;
        protected FontStretch _fontStretch;
        protected Int32 _highlightRule;
        #endregion

        protected VisualLine _owner;
    }
}
