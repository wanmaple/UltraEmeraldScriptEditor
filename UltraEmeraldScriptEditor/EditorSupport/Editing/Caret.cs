using EditorSupport.Document;
using EditorSupport.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace EditorSupport.Editing
{
    /// <summary>
    /// 编辑光标
    /// </summary>
    public class Caret : IRenderable
    {
        #region Properties
        public TextLocation Location { get => _location; set => _location = value; }
        public Point RenderPosition { get => _renderPos; set => _renderPos = value; }
        public Size CaretSize { get => _caretSize; set => _caretSize = value; }
        public Boolean Visible { get => _visible; set => _visible = value; }
        public Brush Foreground { get => _fgBrush; set => _fgBrush = value; }
        public EditView Owner { get => _owner; }
        #endregion

        #region Constructor
        public Caret(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _location = new TextLocation(1, 1);
            _visible = true;
            _fgBrush = Brushes.Black;
            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromSeconds(0.5);
            _blinkTimer.Tick += OnBlinkTimerTick;
            StartAnimation();
        }
        #endregion

        #region Animation
        public void StartAnimation()
        {
            _timerWorking = true;
            _blinkTimer.Start();
        }

        private void OnBlinkTimerTick(object sender, EventArgs e)
        {
            _timerWorking = !_timerWorking;
            _owner.Redraw();
        }

        public void StopAnimation()
        {
            _timerWorking = false;
            _blinkTimer.Stop();
        }

        private DispatcherTimer _blinkTimer;
        private Boolean _timerWorking;
        #endregion

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (!_visible || !_timerWorking)
            {
                return;
            }
            drawingContext.DrawRectangle(_fgBrush, null, new Rect(-_caretSize.Width * 0.5 + _renderPos.X, _renderPos.Y, _caretSize.Width, _caretSize.Height));
        }
        #endregion

        protected TextLocation _location;
        protected Point _renderPos;
        protected Boolean _visible;
        protected Brush _fgBrush;
        protected Size _caretSize;
        protected EditView _owner;
    }
}
