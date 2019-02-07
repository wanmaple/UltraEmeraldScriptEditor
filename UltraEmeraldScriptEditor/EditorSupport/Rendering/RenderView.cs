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

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器的渲染逻辑都在这里
    /// </summary>
    public sealed class RenderView : FrameworkElement, IEditorComponent
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

            return new Size(desireWidth, desireHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ResetVisualRegion(finalSize);
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

            _renderContext.PushTranslation(Padding.Left, Padding.Top);
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
