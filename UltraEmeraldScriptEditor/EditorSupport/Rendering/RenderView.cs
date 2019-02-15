using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using EditorSupport.Document;
using EditorSupport.Editing;
using EditorSupport.Highlighting;
using EditorSupport.Rendering.Renderers;
using EditorSupport.Utils;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器的渲染逻辑都在这里
    /// </summary>
    public class RenderView : FrameworkElement, IWeakEventListener, IScrollInfo, IEditInfo
    {
        #region Properties
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(RenderView), new PropertyMetadata(OnDocumentChanged));
        public static readonly DependencyProperty GlyphOptionProperty =
    DependencyProperty.Register("GlyphOption", typeof(GlyphProperties), typeof(RenderView), new PropertyMetadata(OnGlyphOptionChanged));
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(RenderView), new PropertyMetadata(new Thickness(10, 5, 10, 5), OnPaddingChanged));
        public static readonly DependencyProperty SyntaxProperty =
            DependencyProperty.Register("Syntax", typeof(String), typeof(RenderView), new PropertyMetadata("Plain", OnSyntaxChanged));

        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
        public GlyphProperties GlyphOption
        {
            get { return (GlyphProperties)GetValue(GlyphOptionProperty); }
            set { SetValue(GlyphOptionProperty, value); }
        }
        public Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }
        public String Syntax
        {
            get { return (String)GetValue(SyntaxProperty); }
            set { SetValue(SyntaxProperty, value); }
        }
        [Obsolete("BackgroundRenderer is no longer used.")]
        public List<BackgroundRenderer> BackgroundRenderers
        {
            get { return _bgRenderers; }
        }
        public List<VisualLine> VisualLines
        {
            get { return _allVisualLines; }
        }
        #endregion

        #region Constructor
        public RenderView()
        {
            _renderContext = new RenderContext();
            _bgRenderers = new List<BackgroundRenderer>();
            _lineRenderer = new VisualLineRenderer(this);
            _allVisualLines = new List<VisualLine>();

            _highlighter = HighlightingFactory.GetInstance().GetHighlighter(Syntax);
            _highlightRuler = HighlightingFactory.GetInstance().GetHighlightRuler(Syntax);
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext)
        {
            //RenderBackground(drawingContext);
            RenderLines(drawingContext);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ResetVisualRegion(availableSize);
            _scrollViewport = availableSize;

            // 找出所有需要绘制的VisualLine
            MeasureVisibleLines();

            // 所有VisualLine的逻辑区域
            Double maxLineWidth = 0.0;
            foreach (VisualLine line in _allVisualLines)
            {
                Double visualLen = line.VisualLength;
                if (visualLen > maxLineWidth)
                {
                    maxLineWidth = visualLen;
                }
            }
            Double docHeight = _allVisualLines.Count * GlyphOption.LineHeight;
            Double desireWidth = maxLineWidth + Padding.Left + Padding.Right;
            Double desireHeight = docHeight + Padding.Top + Padding.Bottom + ViewportHeight * 0.5;
            Size desireSize = new Size(desireWidth, desireHeight);

            return desireSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // 滚动区域相关逻辑
            _scrollExtent = finalSize;
            _canHorizontallyScroll = ExtentWidth > ViewportWidth;
            _canVerticallyScroll = ExtentHeight > ViewportHeight;
            _scrollOffset.X = CommonUtilities.Clamp(_scrollOffset.X, 0.0, ExtentWidth - ViewportWidth);
            _scrollOffset.Y = CommonUtilities.Clamp(_scrollOffset.Y, 0.0, ExtentHeight - ViewportHeight);
            if (ScrollOwner != null)
            {
                ScrollOwner.InvalidateScrollInfo();
            }

            base.InvalidateVisual();

            return base.ArrangeOverride(finalSize);
        }
        #endregion

        #region Rendering
        [Obsolete("Background renderers are already obsoleted.")]
        private void RenderBackground(DrawingContext drawingContext)
        {
            foreach (var renderer in BackgroundRenderers)
            {
                renderer.Render(drawingContext, _renderContext);
            }
        }

        private void RenderLines(DrawingContext drawingContext)
        {
            _renderContext.PrepareRendering();

            _renderContext.PushTranslation(Padding.Left + _lineRenderer.RenderOffset.X, Padding.Top + _lineRenderer.RenderOffset.Y);
            _lineRenderer.Render(drawingContext, _renderContext);

            _renderContext.FinishRendering();
        }

        private RenderContext _renderContext;
        private List<BackgroundRenderer> _bgRenderers;
        private VisualLineRenderer _lineRenderer;
        #endregion

        #region Highlighting control
        public IHighlighter Highlighter => _highlighter;
        public IHighlightRuler HighlightRuler => _highlightRuler;

        private IHighlighter _highlighter;
        private IHighlightRuler _highlightRuler;
        #endregion

        #region Visual control
        private void RebuildVisualLines()
        {
            Debug.Assert(Document != null);
            ClearVisualLines();
            Int32 lineNumber = 1;
            while (lineNumber <= Document.LineCount)
            {
                DocumentLine line = Document.GetLineByNumber(lineNumber);
                VisualLine visualLine = new VisualLine(this, Document, line);
                _allVisualLines.Add(visualLine);
                ++lineNumber;
            }
        }

        private void ClearVisualLines()
        {
            foreach (var line in _allVisualLines)
            {
                line.Dispose();
            }
            _allVisualLines.Clear();
            _lineRenderer.VisibleLines.Clear();
        }

        private void ResetVisualRegion(Size availableSize)
        {
            _renderContext.Region = new Rect(availableSize);
        }

        private void MeasureVisibleLines()
        {
            _lineRenderer.VisibleLines.Clear();
            Double relativeOffsetY = VerticalOffset - Padding.Top;
            Int32 startIdx = Math.Max(Convert.ToInt32(Math.Floor(relativeOffsetY / GlyphOption.LineHeight)), 0);
            Int32 endIdx = Math.Min(Convert.ToInt32(Math.Floor((relativeOffsetY + ViewportHeight) / GlyphOption.LineHeight)), _allVisualLines.Count - 1);
            for (int i = startIdx; i <= endIdx; i++)
            {
                _lineRenderer.VisibleLines.AddLast(_allVisualLines[i]);
            }
            _lineRenderer.RenderOffset = new Point(-HorizontalOffset, -((VerticalOffset - Padding.Top) % GlyphOption.LineHeight + Padding.Top));
        }

        private void Redraw()
        {
            InvalidateMeasure(DispatcherPriority.Normal);
        }

        /// <summary>
        /// Measure是异步操作，通过传入的优先级判断是否需要立刻刷新
        /// </summary>
        /// <param name="priority"></param>
        private void InvalidateMeasure(DispatcherPriority priority)
        {
            if (priority >= DispatcherPriority.Render)
            {
                // 需要立刻刷新，强制结束之前未完成的Measure操作
                if (_measureOperation != null)
                {
                    _measureOperation.Abort();
                    _measureOperation = null;
                }
                base.InvalidateMeasure();
            }
            else
            {
                if (_measureOperation != null)
                {
                    _measureOperation.Priority = priority;
                }
                else
                {
                    // 交给Dispatcher去分发Measure操作，可能不能立刻得到反馈
                    _measureOperation = Dispatcher.BeginInvoke(priority, new Action(() =>
                    {
                        _measureOperation = null;
                        base.InvalidateMeasure();
                    }));
                }
            }
        }

        private DispatcherOperation _measureOperation;

        private List<VisualLine> _allVisualLines;
        #endregion

        #region IWeakEventListener
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(TextDocumentWeakEventManager.Changing))
            {
                OnDocumentChanging(sender as TextDocument, e);
                return true;
            }
            else if (managerType == typeof(TextDocumentWeakEventManager.Changed))
            {
                OnDocumentChanged(sender as TextDocument, e as DocumentUpdateEventArgs);
                return true;
            }
            return false;
        }
        #endregion

        #region Weak events
        private void OnDocumentChanging(TextDocument document, EventArgs e)
        {

        }

        private void OnDocumentChanged(TextDocument document, DocumentUpdateEventArgs e)
        {
            foreach (DocumentUpdate update in e.Updates)
            {
                if (update.LineNumberNeedUpdate > 0)
                {
                    VisualLine lineNeedUpdate = _allVisualLines[update.LineNumberNeedUpdate - 1];
                    lineNeedUpdate.Rebuild();
                }
                if (update.RemovedStartLineNumber > 0)
                {
                    for (int i = update.RemovedStartLineNumber - 1; i < update.RemovedStartLineNumber + update.RemovedLineCount - 1; i++)
                    {
                        _allVisualLines[i].Dispose();
                    }
                    _allVisualLines.RemoveRange(update.RemovedStartLineNumber - 1, update.RemovedLineCount);
                }
                if (update.NewStartLineNumber > 0)
                {
                    var newLines = new List<VisualLine>();
                    for (int i = update.NewStartLineNumber; i < update.NewStartLineNumber + update.NewLineCount; i++)
                    {
                        VisualLine newLine = new VisualLine(this, Document, Document.GetLineByNumber(i));
                        newLines.Add(newLine);
                    }
                    _allVisualLines.InsertRange(update.NewStartLineNumber - 1, newLines);
                }
            }
            Redraw();
        }
        #endregion

        #region IScrollInfo
        public bool CanVerticallyScroll { get => _canVerticallyScroll; set => _canVerticallyScroll = value; }
        public bool CanHorizontallyScroll { get => _canHorizontallyScroll; set => _canHorizontallyScroll = value; }

        public double ExtentWidth => _scrollExtent.Width;

        public double ExtentHeight => _scrollExtent.Height;

        public double ViewportWidth => _scrollViewport.Width;

        public double ViewportHeight => _scrollViewport.Height;

        public double HorizontalOffset => _scrollOffset.X;

        public double VerticalOffset => _scrollOffset.Y;

        public ScrollViewer ScrollOwner { get => _scrollOwner; set => _scrollOwner = value; }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + GlyphOption.LineHeight);
        }

        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - GlyphOption.LineHeight);
        }

        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + GlyphOption.LineHeight);
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - GlyphOption.LineHeight);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + GlyphOption.LineHeight * 3);
        }

        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - GlyphOption.LineHeight * 3);
        }

        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + GlyphOption.LineHeight * 3);
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - GlyphOption.LineHeight * 3);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _scrollViewport.Height);
        }

        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - _scrollViewport.Height);
        }

        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset - _scrollViewport.Height);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _scrollViewport.Height);

        }

        public void SetHorizontalOffset(double offset)
        {
            offset = CommonUtilities.Clamp(offset, 0, ExtentWidth - ViewportWidth);
            if (offset != _scrollOffset.X)
            {
                _scrollOffset.X = offset;
                ResetVisualRegion(_renderContext.Region.Size);
                InvalidateMeasure(DispatcherPriority.Normal);
            }
        }

        public void SetVerticalOffset(double offset)
        {
            offset = CommonUtilities.Clamp(offset, 0, ExtentHeight - ViewportHeight);
            if (offset != _scrollOffset.Y)
            {
                _scrollOffset.Y = offset;
                ResetVisualRegion(_renderContext.Region.Size);
                InvalidateMeasure(DispatcherPriority.Normal);
            }
        }

        private ScrollViewer _scrollOwner;
        private Size _scrollViewport;
        private Size _scrollExtent;
        private Point _scrollOffset;
        private Boolean _canVerticallyScroll;
        private Boolean _canHorizontallyScroll;
        #endregion

        #region IEditInfo
        public virtual void ChangeDocument(TextDocument doc)
        {
            if (Document != doc)
            {
                Document = doc;
            }
        }

        public virtual void MeasureCaretRendering(Caret caret)
        {
            if (_lineRenderer.VisibleLines.Count == 0)
            {
                return;
            }
            //Debug.Assert(_lineRenderer.VisibleLines.Count > 0);
            Size caretSize = new Size(1.0, GlyphOption.LineHeight);
            Point caretPos = LocationToPosition(caret.Location);
            Rect caretRect = new Rect(caretPos, caretSize);
            caret.ViewRect = caretRect;
            caretRect.Intersect(_renderContext.Region);
            caret.RenderRect = caretRect;
        }

        public virtual void MeasureSelectionRendering(Selection selection)
        {
            if (_lineRenderer.VisibleLines.Count == 0)
            {
                return;
            }
            //Debug.Assert(_lineRenderer.VisibleLines.Count > 0);
            selection.RenderRects.Clear();
            selection.ViewRects.Clear();
            if (selection.IsEmpty)
            {
                return;
            }
            Rect visualArea = _renderContext.Region;
            TextLocation startLocation = Document.GetLocation(selection.StartOffset);
            TextLocation endLocation = Document.GetLocation(selection.EndOffset);
            if (startLocation.Line == endLocation.Line)
            {
                // 单行
                Point viewStart = LocationToPosition(startLocation);
                Point viewEnd = LocationToPosition(endLocation);
                viewEnd.Y += GlyphOption.LineHeight;
                var viewRect = new Rect(viewStart, viewEnd);
                selection.ViewRects.Add(viewRect);
                viewRect.Intersect(visualArea);
                if (!viewRect.IsEmpty)
                {
                    selection.RenderRects.Add(viewRect);
                }
            }
            else
            {
                Point viewStart, viewEnd;
                Rect viewRect;
                // 第一行到底
                viewStart = LocationToPosition(startLocation);
                VisualLine line = _allVisualLines[startLocation.Line - 1];
                viewEnd = new Point(line.VisualLength + CommonUtilities.LineVisualUnit * GlyphOption.FontSize - HorizontalOffset, viewStart.Y + GlyphOption.LineHeight);
                viewRect = new Rect(viewStart, viewEnd);
                selection.ViewRects.Add(viewRect);
                viewRect.Intersect(_renderContext.Region);
                if (!viewRect.IsEmpty)
                {
                    selection.RenderRects.Add(viewRect);
                }
                // 中间若干行
                Int32 lineNum = startLocation.Line + 1;
                while (lineNum != endLocation.Line)
                {
                    line = _allVisualLines[lineNum - 1];
                    viewStart = new Point(Padding.Left - HorizontalOffset, viewStart.Y + GlyphOption.LineHeight);
                    viewEnd = new Point(line.VisualLength + CommonUtilities.LineVisualUnit * GlyphOption.FontSize - HorizontalOffset, viewEnd.Y + GlyphOption.LineHeight);
                    viewRect = new Rect(viewStart, viewEnd);
                    selection.ViewRects.Add(viewRect);
                    viewRect.Intersect(visualArea);
                    if (!viewRect.IsEmpty)
                    {
                        selection.RenderRects.Add(viewRect);
                    }
                    ++lineNum;
                }
                // 最后一行到头
                viewStart = new Point(Padding.Left - HorizontalOffset, viewStart.Y + GlyphOption.LineHeight);
                viewEnd = LocationToPosition(endLocation);
                viewEnd.Y += GlyphOption.LineHeight;
                viewRect = new Rect(viewStart, viewEnd);
                selection.ViewRects.Add(viewRect);
                viewRect.Intersect(visualArea);
                if (!viewRect.IsEmpty)
                {
                    selection.RenderRects.Add(viewRect);
                }
            }
        }

        public virtual void MeasureCaretLocation(Caret caret, Point positionToView)
        {
            caret.DocumentOffset = Document.GetOffset(PositionToLocation(positionToView));
        }

        public virtual Tuple<Int32, Int32> MeasureWordOffsets(Int32 currentOffset)
        {
            Nullable<Boolean> isLeftSplitter = null, isRightSplitter = null;
            // 左侧
            Int32 wordLeft = WordLeftCaretOffset(currentOffset);
            if (wordLeft != currentOffset)
            {
                isLeftSplitter = IsSplitter(Document.GetCharacterAt(currentOffset - 1));
            }
            // 右侧
            Int32 wordRight = WordRightCaretOffset(currentOffset);
            if (wordRight != currentOffset)
            {
                isRightSplitter = IsSplitter(Document.GetCharacterAt(currentOffset));
            }
            if (isLeftSplitter == null && isRightSplitter == null)
            {
                // 左侧右侧都没有（空行）
                return new Tuple<int, int>(currentOffset, currentOffset);
            }
            if (isLeftSplitter != null && isRightSplitter != null)
            {
                if (isLeftSplitter != isRightSplitter)
                {
                    // 两侧类型不同优先取右侧
                    return new Tuple<int, int>(currentOffset, wordRight);
                }
                return new Tuple<int, int>(wordLeft, wordRight);
            }
            if (isLeftSplitter != null)
            {
                // 左侧有
                return new Tuple<int, int>(wordLeft, currentOffset);
            }
            return new Tuple<int, int>(currentOffset, wordRight);
        }

        public virtual Int32 WordLeftCaretOffset(Int32 currentOffset)
        {
            // 规则是不跨行选择
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            if (currentLocation.Column == 1)
            {
                return currentOffset;
            }
            Int32 lineStart = currentLine.Line.StartOffset;
            String text = Document.GetTextAt(lineStart, currentOffset - lineStart);
            ITextSource source = new StringTextSource(text);
            Int32 targetIdx = source.Length - 1;
            Char firstChar = source.GetCharacterAt(targetIdx);
            Boolean isSplitter = IsSplitter(firstChar);
            --targetIdx;
            while (targetIdx >= 0)
            {
                Char ch = source.GetCharacterAt(targetIdx);
                if (IsSplitter(ch) != isSplitter)
                {
                    break;
                }
                --targetIdx;
            }
            return lineStart + targetIdx + 1;
        }

        public virtual Int32 WordRightCaretOffset(Int32 currentOffset)
        {
            // 规则是不跨行选择
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            if (currentLocation.Column == currentLine.Line.Length + 1)
            {
                return currentOffset;
            }
            Int32 lineEnd = currentLine.Line.EndOffset;
            String text = Document.GetTextAt(currentOffset, lineEnd - currentOffset);
            ITextSource source = new StringTextSource(text);
            Int32 targetIdx = 0;
            Char firstChar = source.GetCharacterAt(targetIdx);
            Boolean isSplitter = IsSplitter(firstChar);
            ++targetIdx;
            while (targetIdx < source.Length)
            {
                Char ch = source.GetCharacterAt(targetIdx);
                if (IsSplitter(ch) != isSplitter)
                {
                    break;
                }
                ++targetIdx;
            }
            return currentOffset + targetIdx;
        }

        public virtual Int32 LineUpCaretOffset(Int32 currentOffset)
        {
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            if (currentLocation.Line <= 1)
            {
                // 不变化
                return currentOffset;
            }
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            Double currentVisualLength = currentLine.CharacterVisualOffsets.GetSumValue(currentLocation.Column - 1);
            VisualLine prevLine = _allVisualLines[currentLocation.Line - 2];
            // 先预估一个值以免全部遍历
            Int32 estimation = Math.Min(currentLocation.Column - 1, prevLine.Line.Length);
            Double visualLength = prevLine.CharacterVisualOffsets.GetSumValue(estimation);
            Int32 column = estimation;
            if (visualLength < currentVisualLength)
            {
                for (int i = estimation + 1; i <= prevLine.Line.Length; i++)
                {
                    if (prevLine.CharacterVisualOffsets.GetSumValue(i) > currentVisualLength)
                    {
                        column = i - 1;
                        break;
                    }
                }
            }
            else if (visualLength > currentVisualLength)
            {
                for (int i = estimation - 1; i >= 0; --i)
                {
                    if (prevLine.CharacterVisualOffsets.GetSumValue(i) < currentVisualLength)
                    {
                        column = i + 1;
                        break;
                    }
                }
            }
            TextLocation targetLocation = new TextLocation(currentLocation.Line - 1, column + 1);
            return Document.GetOffset(targetLocation);
        }

        public virtual Int32 LineDownCaretOffset(Int32 currentOffset)
        {
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            if (currentLocation.Line >= Document.LineCount)
            {
                // 不变化
                return currentOffset;
            }
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            Double currentVisualLength = currentLine.CharacterVisualOffsets.GetSumValue(currentLocation.Column - 1);
            VisualLine nextLine = _allVisualLines[currentLocation.Line];
            // 先预估一个值以免全部遍历
            Int32 estimation = Math.Min(currentLocation.Column - 1, nextLine.Line.Length);
            Double visualLength = nextLine.CharacterVisualOffsets.GetSumValue(estimation);
            Int32 column = estimation;
            if (visualLength < currentVisualLength)
            {
                for (int i = estimation + 1; i <= nextLine.Line.Length; i++)
                {
                    if (nextLine.CharacterVisualOffsets.GetSumValue(i) > currentVisualLength)
                    {
                        column = i - 1;
                        break;
                    }
                }
            }
            else if (visualLength > currentVisualLength)
            {
                for (int i = estimation - 1; i >= 0; --i)
                {
                    if (nextLine.CharacterVisualOffsets.GetSumValue(i) < currentVisualLength)
                    {
                        column = i + 1;
                        break;
                    }
                }
            }
            TextLocation targetLocation = new TextLocation(currentLocation.Line + 1, column + 1);
            return Document.GetOffset(targetLocation);
        }

        public virtual Int32 PageUpCaretOffset(Int32 currentOffset)
        {
            Int32 pageLines = Convert.ToInt32(Math.Floor(ViewportHeight / GlyphOption.LineHeight));
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            if (currentLocation.Line == 1)
            {
                // 不变化
                return currentOffset;
            }
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            Double currentVisualLength = currentLine.CharacterVisualOffsets.GetSumValue(currentLocation.Column - 1);
            Int32 targetLineNum = Math.Max(currentLocation.Line - pageLines, 1);
            VisualLine targetLine = _allVisualLines[targetLineNum - 1];
            // 先预估一个值以免全部遍历
            Int32 estimation = Math.Min(currentLocation.Column - 1, targetLine.Line.Length);
            Double visualLength = targetLine.CharacterVisualOffsets.GetSumValue(estimation);
            Int32 column = estimation;
            if (visualLength < currentVisualLength)
            {
                for (int i = estimation + 1; i <= targetLine.Line.Length; i++)
                {
                    if (targetLine.CharacterVisualOffsets.GetSumValue(i) > currentVisualLength)
                    {
                        column = i - 1;
                        break;
                    }
                }
            }
            else if (visualLength > currentVisualLength)
            {
                for (int i = estimation - 1; i >= 0; --i)
                {
                    if (targetLine.CharacterVisualOffsets.GetSumValue(i) < currentVisualLength)
                    {
                        column = i + 1;
                        break;
                    }
                }
            }
            TextLocation targetLocation = new TextLocation(targetLineNum, column + 1);
            return Document.GetOffset(targetLocation);
        }

        public virtual Int32 PageDownCaretOffset(Int32 currentOffset)
        {
            Int32 pageLines = Convert.ToInt32(Math.Floor(ViewportHeight / GlyphOption.LineHeight));
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            if (currentLocation.Line == Document.LineCount)
            {
                // 不变化
                return currentOffset;
            }
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            Double currentVisualLength = currentLine.CharacterVisualOffsets.GetSumValue(currentLocation.Column - 1);
            Int32 targetLineNum = Math.Min(currentLocation.Line + pageLines, Document.LineCount);
            VisualLine targetLine = _allVisualLines[targetLineNum - 1];
            // 先预估一个值以免全部遍历
            Int32 estimation = Math.Min(currentLocation.Column - 1, targetLine.Line.Length);
            Double visualLength = targetLine.CharacterVisualOffsets.GetSumValue(estimation);
            Int32 column = estimation;
            if (visualLength < currentVisualLength)
            {
                for (int i = estimation + 1; i <= targetLine.Line.Length; i++)
                {
                    if (targetLine.CharacterVisualOffsets.GetSumValue(i) > currentVisualLength)
                    {
                        column = i - 1;
                        break;
                    }
                }
            }
            else if (visualLength > currentVisualLength)
            {
                for (int i = estimation - 1; i >= 0; --i)
                {
                    if (targetLine.CharacterVisualOffsets.GetSumValue(i) < currentVisualLength)
                    {
                        column = i + 1;
                        break;
                    }
                }
            }
            TextLocation targetLocation = new TextLocation(targetLineNum, column + 1);
            return Document.GetOffset(targetLocation);
        }
        public virtual Int32 WheelUpCaretOffset(Int32 currentOffset)
        {
            Int32 finalOffset = currentOffset;
            for (int i = 0; i < 3; i++)
            {
                finalOffset = LineUpCaretOffset(finalOffset);
            }
            return finalOffset;
        }

        public virtual Int32 WheelDownCaretOffset(Int32 currentOffset)
        {
            Int32 finalOffset = currentOffset;
            for (int i = 0; i < 3; i++)
            {
                finalOffset = LineDownCaretOffset(finalOffset);
            }
            return finalOffset;
        }

        public virtual Int32 WheelLeftCaretOffset(Int32 currentOffset)
        {
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            if (currentLocation.Column == 1)
            {
                return currentOffset;
            }
            Double visualOffset = GlyphOption.LineHeight * 3.0;
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            Int32 targetColumn = currentLocation.Column - 1;
            while (visualOffset > 0)
            {
                visualOffset -= currentLine.CharacterVisualOffsets[targetColumn - 1];
                if (targetColumn == 1)
                {
                    break;
                }
                --targetColumn;
            }
            return Document.GetOffset(new TextLocation(currentLocation.Line, targetColumn));
        }

        public virtual Int32 WheelRightCaretOffset(Int32 currentOffset)
        {
            TextLocation currentLocation = Document.GetLocation(currentOffset);
            VisualLine currentLine = _allVisualLines[currentLocation.Line - 1];
            if (currentLocation.Column == currentLine.Line.Length + 1)
            {
                return currentOffset;
            }
            Double visualOffset = GlyphOption.LineHeight * 3.0;
            Int32 targetColumn = currentLocation.Column + 1;
            while (visualOffset > 0)
            {
                visualOffset -= currentLine.CharacterVisualOffsets[targetColumn - 1];
                if (targetColumn == currentLine.Line.Length + 1)
                {
                    break;
                }
                ++targetColumn;
            }
            return Document.GetOffset(new TextLocation(currentLocation.Line, targetColumn));
        }

        protected virtual Boolean IsSplitter(Char ch)
        {
            return SPLITTERS.Contains(ch);
        }

        private Point LocationToPosition(TextLocation location)
        {
            Debug.Assert(_lineRenderer.VisibleLines.Count > 0);
            Int32 renderFirstLineNum = _lineRenderer.VisibleLines.First.Value.Line.LineNumber;
            Int32 renderLastLineNum = _lineRenderer.VisibleLines.Last.Value.Line.LineNumber;
            Int32 visualLineIdx = location.Line - renderFirstLineNum;
            VisualLine caretLine = _allVisualLines[location.Line - 1];
            Double caretVisualOffset = caretLine.CharacterVisualOffsets.GetSumValue(location.Column - 1);
            Double caretPosX = caretVisualOffset + _lineRenderer.RenderOffset.X;
            Double caretPosY = _lineRenderer.RenderOffset.Y + visualLineIdx * GlyphOption.LineHeight;

            return new Point(caretPosX + Padding.Left, caretPosY + Padding.Top);
        }

        private TextLocation PositionToLocation(Point position)
        {
            position.X -= _lineRenderer.RenderOffset.X + Padding.Left;
            position.Y -= Padding.Top;
            Debug.Assert(_lineRenderer.VisibleLines.Count > 0);
            Int32 renderFirstLineNum = _lineRenderer.VisibleLines.First.Value.Line.LineNumber;
            Int32 renderLastLineNum = _lineRenderer.VisibleLines.Last.Value.Line.LineNumber;
            Double startY = _lineRenderer.RenderOffset.Y;
            Double endY = startY + _lineRenderer.VisibleLines.Count * GlyphOption.LineHeight;
            Int32 line = CommonUtilities.Clamp(Convert.ToInt32(Math.Ceiling((position.Y - startY) / GlyphOption.LineHeight)) - 1 + renderFirstLineNum, renderFirstLineNum, renderLastLineNum);
            VisualLine visualLine = _lineRenderer.VisibleLines.ElementAt(line - renderFirstLineNum);
            Double visualOffset = Padding.Left;
            Int32 column = 1;
            while (position.X >= visualOffset && column <= visualLine.CharacterVisualOffsets.Count)
            {
                Double chLen = visualLine.CharacterVisualOffsets[column - 1];
                if (position.X <= visualOffset + chLen * 0.5)
                {
                    break;
                }
                visualOffset += chLen;
                ++column;
            }

            return new TextLocation(line, column);
        }

        private readonly Char[] SPLITTERS = { ' ', ',', '.', '=', '+', '-', '*', '/', ':', ';', '(', ')', };
        #endregion

        #region PropertyChange EventHandlers
        private static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            RenderView editor = dp as RenderView;
            if (e.OldValue != null)
            {
                VisualLineImageCache.GetInstance().RemoveCache(e.OldValue as TextDocument);
            }
            editor.OnDocumentChanged(e.OldValue as TextDocument, e.NewValue as TextDocument);
        }
        private void OnDocumentChanged(TextDocument oldDoc, TextDocument newDoc)
        {
            if (oldDoc != null)
            {
                TextDocumentWeakEventManager.Changing.RemoveListener(oldDoc, this);
                TextDocumentWeakEventManager.Changed.RemoveListener(oldDoc, this);
            }
            if (newDoc != null)
            {
                TextDocumentWeakEventManager.Changing.AddListener(newDoc, this);
                TextDocumentWeakEventManager.Changed.AddListener(newDoc, this);
            }
            RebuildVisualLines();
            MeasureVisibleLines();
            Redraw();
        }

        private static void OnGlyphOptionChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            RenderView editor = dp as RenderView;
            if (e.OldValue != null)
            {
                (e.OldValue as GlyphProperties).OptionChanged -= editor.OnGlyphOptionChanged;
            }
            if (e.NewValue != null)
            {
                (e.NewValue as GlyphProperties).OptionChanged += editor.OnGlyphOptionChanged;
            }
        }
        private void OnGlyphOptionChanged(Object sender, EventArgs e)
        {
            Redraw();
        }

        private static void OnPaddingChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            RenderView editor = dp as RenderView;
            editor.OnPaddingChanged(e.NewValue, EventArgs.Empty);
        }
        private void OnPaddingChanged(Object sender, EventArgs e)
        {
            Redraw();
        }

        private static void OnSyntaxChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            RenderView editor = dp as RenderView;
            editor.OnSyntaxChanged(e.NewValue, EventArgs.Empty);
        }
        private void OnSyntaxChanged(Object sender, EventArgs e)
        {
            String newSyntax = sender as String;
            _highlighter = HighlightingFactory.GetInstance().GetHighlighter(newSyntax);
            _highlightRuler = HighlightingFactory.GetInstance().GetHighlightRuler(newSyntax);
            Redraw();
        }
        #endregion
    }
}
