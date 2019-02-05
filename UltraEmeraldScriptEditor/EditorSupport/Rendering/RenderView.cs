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
    public sealed class RenderView : FrameworkElement, IEditorComponent
    {
        #region Properties
        public static readonly DependencyProperty GlyphOptionProperty =
    DependencyProperty.Register("GlyphOption", typeof(GlyphProperties), typeof(RenderView), new PropertyMetadata(OnGlyphOptionChanged));
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(RenderView), new PropertyMetadata(new Thickness(20, 10, 20, 10)));

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
            _bgRenderers = new ObservableCollection<BackgroundRenderer>();
            _lineRenderer = new VisualLineRenderer(this);
            _allVisualLines = new List<VisualLine>();
            RebuildVisualLines();
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext)
        {
            RenderBackground(drawingContext);
            _lineRenderer.Render(drawingContext, this);
            //var drawingGroup = new DrawingGroup();
            //Double totalWidth;
            //drawingGroup.Children.Add(CreateGlyphRunDrawing("this ", Brushes.Blue, new Point(0, 32), out totalWidth));
            //drawingGroup.Children.Add(CreateGlyphRunDrawing("万鑫", Brushes.Black, new Point(totalWidth, 32), out totalWidth));
            //var img = new DrawingImage(drawingGroup);
            //drawingContext.DrawImage(img, new Rect(new Point(50, 50), new Point(50 + img.Width, 50 + img.Height)));
        }
        #endregion

        private GlyphRunDrawing CreateGlyphRunDrawing(String text, Brush foreBrush, Point pos, out Double totalWidth)
        {
            var defaultTypeface = new Typeface(new FontFamily("Microsoft YaHei"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            GlyphTypeface defaultTF;
            defaultTypeface.TryGetGlyphTypeface(out defaultTF);

            totalWidth = 0.0;
            var fontFamily = new FontFamily("Microsoft YaHei");
            Typeface typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            GlyphTypeface glyphTypeface;
            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                return null;
            }
            UInt16[] glyphIdxes = new ushort[text.Length];
            Double[] advanceWidths = new double[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                UInt16 glyphIdx;
                Double width;
                Char ch = text[i];
                try
                {
                    glyphIdx = glyphTypeface.CharacterToGlyphMap[ch];
                    width = glyphTypeface.AdvanceWidths[glyphIdx] * 32.0;
                }
                catch (Exception)
                {
                    glyphIdx = defaultTF.CharacterToGlyphMap[ch];
                    width = defaultTF.AdvanceWidths[glyphIdx] * 32.0;
                }
                glyphIdxes[i] = glyphIdx;
                advanceWidths[i] = width;
                totalWidth += width;
            }
            GlyphRun gr = new GlyphRun(glyphTypeface, 0, false, 32.0, glyphIdxes, pos, advanceWidths, null, null, null, null, null, null);
            var grDrawing = new GlyphRunDrawing(foreBrush, gr);
            return grDrawing;
        }

        private void RenderBackground(DrawingContext drawingContext)
        {
            foreach (var renderer in BackgroundRenderers)
            {
                renderer.Render(drawingContext, this);
            }
        }

        #region IEditorComponent
        public event EventHandler DocumentChanged;

        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(TextDocument), typeof(RenderView), new PropertyMetadata(new TextDocument("Hello World!\r\n这是一段中文。\r\nThis is 什么？"), OnDocumentChanged));
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
            DocumentLine line = null;
            while (lineNumber <= Document.LineCount && (line = Document.GetLineByNumber(lineNumber)) != null)
            {
                VisualLine visualLine = new VisualLine(Document, line);
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
