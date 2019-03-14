using EditorSupport.Document;
using EditorSupport.Rendering;
using EditorSupport.Rendering.Renderers;
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
    /// 编辑光标。
    /// </summary>
    public class Caret : IRenderable
    {
        public event EventHandler PositionChanged;

        #region Properties
        public Int32 DocumentOffset
        {
            get => _docOffset;
            set
            {
                if (_docOffset != value)
                {
                    _docOffset = value;
                    if (PositionChanged != null)
                    {
                        PositionChanged(this, EventArgs.Empty);
                    }
                }
            }
        }
        public TextLocation Location { get => _owner.Document.GetLocation(_docOffset); }
        /// <summary>
        /// 绘制矩形
        /// </summary>
        public Rect RenderRect { get => _renderRect; set => _renderRect = value; }
        /// <summary>
        /// 实际的UI坐标下的矩形
        /// </summary>
        public Rect ViewRect { get => _viewRect; set => _viewRect = value; }
        /// <summary>
        /// 是否处于Focus状态
        /// </summary>
        public Boolean Visible { get => _visible; set => _visible = value; }
        public Brush Foreground { get => _fgBrush; set => _fgBrush = value; }
        public EditView Owner { get => _owner; }
        #endregion

        #region Constructor
        public Caret(EditView owner)
        {
            _owner = owner ?? throw new ArgumentNullException("owner");
            _visible = true;
            _fgBrush = Brushes.Black;
            _blinkTimer = new DispatcherTimer();
            _blinkTimer.Interval = TimeSpan.FromSeconds(0.5);
            _blinkTimer.Tick += OnBlinkTimerTick;
            StartAnimation();
        }
        #endregion

        public void MoveLeft(Int32 length = 1)
        {
            if (length < 0)
            {
                throw new ArgumentException("'length' must be positive.");
            }
            DocumentOffset = Math.Max(_docOffset - length, 0);
        }

        public void MoveRight(Int32 length = 1)
        {
            if (length < 0)
            {
                throw new ArgumentException("'length' must be positive.");
            }
            DocumentOffset = Math.Min(_docOffset + length, _owner.Document.Length);
        }

        #region Animation
        public void StartAnimation()
        {
            _timerWorking = true;
            _blinkTimer.Start();
        }

        public void StopAnimation()
        {
            _timerWorking = false;
            _blinkTimer.Stop();
        }

        public void RestartAnimation()
        {
            StopAnimation();
            StartAnimation();
        }

        private void OnBlinkTimerTick(object sender, EventArgs e)
        {
            _timerWorking = !_timerWorking;
            _owner.Redraw();
        }

        private DispatcherTimer _blinkTimer;
        private Boolean _timerWorking;
        #endregion

        #region IRenderable
        public void Render(DrawingContext drawingContext, RenderContext renderContext)
        {
            if (!_visible || !_timerWorking || _renderRect.IsEmpty)
            {
                return;
            }
            Double renderX = Math.Round(-_renderRect.Width * 0.5 + _renderRect.X);
            Double renderY = _renderRect.Y;
            drawingContext.DrawRectangle(_fgBrush, null, new Rect(renderX, renderY, _renderRect.Width, _renderRect.Height));
        }
        #endregion

        protected Int32 _docOffset;
        protected Boolean _visible;
        protected Brush _fgBrush;
        protected Rect _renderRect;
        protected Rect _viewRect;
        protected EditView _owner;
    }
}
