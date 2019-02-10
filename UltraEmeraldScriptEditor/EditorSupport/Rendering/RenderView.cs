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
using EditorSupport.Highlighting;
using EditorSupport.Utils;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器的渲染逻辑都在这里
    /// </summary>
    public sealed class RenderView : FrameworkElement, IEditorComponent, IScrollInfo
    {
        #region Properties
        public static readonly DependencyProperty GlyphOptionProperty =
    DependencyProperty.Register("GlyphOption", typeof(GlyphProperties), typeof(RenderView), new PropertyMetadata(OnGlyphOptionChanged));
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(RenderView), new PropertyMetadata(new Thickness(10, 5, 10, 5), OnPaddingChanged));
        public static readonly DependencyProperty SyntaxProperty =
            DependencyProperty.Register("Syntax", typeof(String), typeof(RenderView), new PropertyMetadata("Plain", OnSyntaxChanged));

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
        public List<BackgroundRenderer> BackgroundRenderers
        {
            get { return _bgRenderers; }
        }
        public ObservableCollection<VisualLine> VisualLines
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
            _allVisualLines = new ObservableCollection<VisualLine>();

            _highlighter = HighlightingFactory.GetInstance().GetHighlighter(Syntax);
            _highlightRuler = HighlightingFactory.GetInstance().GetHighlightRuler(Syntax);
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext)
        {
            RenderBackground(drawingContext);
            RenderLines(drawingContext);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _scrollViewport = availableSize;

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
            Double desireHeight = docHeight + Padding.Top + Padding.Bottom;
            Size desireSize = new Size(desireWidth, desireHeight);
            // 找出所有需要绘制的VisualLine
            _lineRenderer.VisibleLines.Clear();
            Double relativeOffsetY = VerticalOffset;
            Int32 startIdx = Math.Max(Convert.ToInt32(Math.Floor(relativeOffsetY / GlyphOption.LineHeight)), 0);
            Int32 endIdx = Math.Min(Convert.ToInt32(Math.Ceiling((relativeOffsetY + ViewportHeight) / GlyphOption.LineHeight)), _allVisualLines.Count - 1);
            for (int i = startIdx; i < endIdx; i++)
            {
                _lineRenderer.VisibleLines.AddLast(_allVisualLines[i]);
            }
            _lineRenderer.RenderOffset = new Point(-HorizontalOffset, -(VerticalOffset % GlyphOption.LineHeight));

            return desireSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ResetVisualRegion(finalSize);
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
        #endregion

        #region IEditorComponent
        public event EventHandler DocumentChanged;

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(RenderView), new PropertyMetadata(new TextDocument(), OnDocumentChanged));
        public TextDocument Document
        {
            get { return (TextDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }
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
                _lineRenderer.VisibleLines.AddLast(visualLine);
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

        private RenderContext _renderContext;
        private List<BackgroundRenderer> _bgRenderers;
        private VisualLineRenderer _lineRenderer;
        private ObservableCollection<VisualLine> _allVisualLines;
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
                InvalidateMeasure(DispatcherPriority.Normal);
            }
        }

        public void SetVerticalOffset(double offset)
        {
            offset = CommonUtilities.Clamp(offset, 0, ExtentHeight - ViewportHeight);
            if (offset != _scrollOffset.Y)
            {
                _scrollOffset.Y = offset;
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

        #region PropertyChange EventHandlers
        private static void OnDocumentChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            RenderView editor = dp as RenderView;
            editor.OnDocumentChanged();
        }
        private void OnDocumentChanged()
        {
            if (DocumentChanged != null)
            {
                DocumentChanged(this, EventArgs.Empty);
            }
            RebuildVisualLines();
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

        //private static void OnBackgroundRenderersChanged(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        //{
        //    if (e.OldValue != null)
        //    {
        //        var handler = (INotifyCollectionChanged)e.OldValue;
        //        handler.CollectionChanged -= OnBackgroundRendererCollectionChanged;
        //    }
        //    if (e.NewValue != null)
        //    {
        //        var handler = (INotifyCollectionChanged)e.NewValue;
        //        handler.CollectionChanged += OnBackgroundRendererCollectionChanged;
        //    }
        //}
        //private static void OnBackgroundRendererCollectionChanged(Object sender, NotifyCollectionChangedEventArgs e)
        //{
        //}
    }
}
