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
using EditorSupport.Document;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 编辑器的渲染逻辑都在这里
    /// </summary>
    public sealed class RenderView : FrameworkElement, IScrollInfo, IEditorComponent
    {
        #region Properties
        public static readonly DependencyProperty GlyphOptionProperty =
    DependencyProperty.Register("GlyphOption", typeof(GlyphProperties), typeof(RenderView), new PropertyMetadata(OnGlyphOptionChanged));
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(RenderView), new PropertyMetadata(new Thickness(10, 5, 10, 5), OnPaddingChanged));

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
        public ObservableCollection<BackgroundRenderer> BackgroundRenderers
        {
            get { return _bgRenderers; }
        }
        #endregion

        #region Constructor
        public RenderView()
        {
            _renderContext = new RenderContext();
            _bgRenderers = new ObservableCollection<BackgroundRenderer>();
            _lineRenderer = new VisualLineRenderer(this);
            _allVisualLines = new List<VisualLine>();
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext)
        {
            ResetRenderRegion();
            RenderBackground(drawingContext);
            RenderLines(drawingContext);
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

        #region IScrollInfo
        public bool CanVerticallyScroll { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanHorizontallyScroll { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public double ExtentWidth => throw new NotImplementedException();

        public double ExtentHeight => throw new NotImplementedException();

        public double ViewportWidth => throw new NotImplementedException();

        public double ViewportHeight => throw new NotImplementedException();

        public double HorizontalOffset => throw new NotImplementedException();

        public double VerticalOffset => throw new NotImplementedException();

        public ScrollViewer ScrollOwner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void LineDown()
        {
            throw new NotImplementedException();
        }

        public void LineLeft()
        {
            throw new NotImplementedException();
        }

        public void LineRight()
        {
            throw new NotImplementedException();
        }

        public void LineUp()
        {
            throw new NotImplementedException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            throw new NotImplementedException();
        }

        public void MouseWheelDown()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelLeft()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelRight()
        {
            throw new NotImplementedException();
        }

        public void MouseWheelUp()
        {
            throw new NotImplementedException();
        }

        public void PageDown()
        {
            throw new NotImplementedException();
        }

        public void PageLeft()
        {
            throw new NotImplementedException();
        }

        public void PageRight()
        {
            throw new NotImplementedException();
        }

        public void PageUp()
        {
            throw new NotImplementedException();
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset)
        {
            throw new NotImplementedException();
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

        private void RebuildVisualLines()
        {
            Debug.Assert(Document != null);
            ClearVisualLines();
            Int32 lineNumber = 1;
            while (lineNumber <= Document.LineCount)
            {
                DocumentLine line = Document.GetLineByNumber(lineNumber);
                VisualLine visualLine = new VisualLine(Document, line, GlyphOption);
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

        private void ResetRenderRegion()
        {
            _renderContext.Region = new Rect(new Point(0, 0), new Size(ActualWidth, ActualHeight));
        }

        private void Redraw()
        {
            base.InvalidateVisual();
        }

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
            Thickness padding = (Thickness)e.NewValue;
            editor.OnPaddingChanged(padding, EventArgs.Empty);
        }
        private void OnPaddingChanged(Object sender, EventArgs e)
        {
            Redraw();
        }

        private RenderContext _renderContext;
        private ObservableCollection<BackgroundRenderer> _bgRenderers;
        private VisualLineRenderer _lineRenderer;
        private List<VisualLine> _allVisualLines;

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
