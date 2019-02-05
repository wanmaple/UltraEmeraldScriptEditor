using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 方便临时使用的片段对象。
    /// </summary>
    internal sealed class SimpleSegment : ISegment, IEquatable<SimpleSegment>
    {
        public static SimpleSegment Invalid = new SimpleSegment(-1, -1);

        #region Constructor
        public SimpleSegment(Int32 offset, Int32 length)
        {
            _offset = offset;
            _length = length;
        } 
        #endregion

        #region ISegment
        public int StartOffset
        {
            get { return _offset; }
        }

        public int Length
        {
            get { return _length; }
        }

        public int EndOffset
        {
            get { return _offset + _length; }
        }
        #endregion

        #region IEquatable<SimpleSegment>
        public bool Equals(SimpleSegment other)
        {
            return _offset == other._offset && _length == other._length;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            return (obj is SimpleSegment) && Equals(obj as SimpleSegment);
        }

        public override string ToString()
        {
            return String.Format("[offset={0},length={1}]", _offset, _length);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return _offset + 10301 * Length;
            }
        }
        #endregion

        #region Operators
        public static Boolean operator ==(SimpleSegment left, SimpleSegment right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(SimpleSegment left, SimpleSegment right)
        {
            return !(left == right);
        }
        #endregion

        private readonly Int32 _offset;
        private readonly Int32 _length;
    }
}
