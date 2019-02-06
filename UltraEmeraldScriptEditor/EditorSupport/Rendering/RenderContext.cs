using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace EditorSupport.Rendering
{
    /// <summary>
    /// 渲染环境
    /// </summary>
    /// <remarks>
    /// 由于大部分都是自定义渲染，因此添加了这个对象存储一些渲染中的变化。
    /// </remarks>
    public sealed class RenderContext
    {
        private class RenderSegment
        {
            public TransformGroup Transform { get; set; }

            public RenderSegment(TransformGroup parent)
            {
                Transform = parent.Clone();
            }
        }
        /// <summary>
        /// 渲染区域
        /// </summary>
        public Rect Region { get; set; }

        /// <summary>
        /// 当前环境的总变换
        /// </summary>
        public Transform Transform
        {
            get => _transform;
        }

        /// <summary>
        /// 快捷获取当前环境变换的总偏移
        /// </summary>
        public Point Offset
        {
            get => new Point(Transform.Value.OffsetX, Transform.Value.OffsetY);
        }

        public RenderContext()
        {
            Region = Rect.Empty;
            _segStack = new Stack<RenderSegment>();
            _transform = new TransformGroup();
        }

        public void PrepareRendering()
        {
            _segStack.Push(new RenderSegment(_transform));
        }

        public void FinishRendering()
        {
            RenderSegment rlv = _segStack.Pop();
            _transform = rlv.Transform;
        }

        public void PushTranslation(Double dx, Double dy)
        {
            var newTransform = new TranslateTransform(dx, dy);
            _transform.Children.Add(newTransform);
        }

        public void PushRotation(Double angle, Double centerX = 0.0, Double centerY = 0.0)
        {
            var newTransform = new RotateTransform(angle, centerX, centerY);
            _transform.Children.Add(newTransform);
        }

        public void PushScaling(Double scale)
        {
            var newTransform = new ScaleTransform(scale, scale);
            _transform.Children.Add(newTransform);
        }

        public void PushScaling(Double scaleX, Double scaleY)
        {
            var newTransform = new ScaleTransform(scaleX, scaleY);
            _transform.Children.Add(newTransform);
        }

        public void PushTransform(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException("transform");
            }
            _transform.Children.Add(transform);
        }

        private Stack<RenderSegment> _segStack;
        private TransformGroup _transform;
    }
}
