using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 表示Document中的位置信息。
    /// </summary>
    public struct TextLocation : IComparable<TextLocation>, IEquatable<TextLocation>
    {
        public Int32 Line
        {
            get { return _x; }
            set { _x = value; }
        }
        public Int32 Column
        {
            get { return _y; }
            set { _y = value; }
        }

        #region Constructor
        public TextLocation(Int32 line, Int32 column)
        {
            if (line <= 0)
            {
                throw new ArgumentOutOfRangeException("line");
            }
            if (column <= 0)
            {
                throw new ArgumentOutOfRangeException("column");
            }
            _x = line;
            _y = column;
        }
        #endregion

        #region Overrides
        public override string ToString()
        {
            return String.Format("({0}, {1})", _x, _y);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextLocation))
            {
                return false;
            }
            return this == (TextLocation)obj;
        }

        public override int GetHashCode()
        {
            return unchecked(131 * _x.GetHashCode() ^ _y.GetHashCode());
        }
        #endregion

        #region IComparable<TextLocation>
        public int CompareTo(TextLocation other)
        {
            if (this == other)
            {
                return 0;
            }
            return this > other ? 1 : -1;
        }
        #endregion

        #region IEquatable<TextLocation>
        public bool Equals(TextLocation other)
        {
            return this == other;
        }
        #endregion

        #region Operators
        public static Boolean operator ==(TextLocation left, TextLocation right)
        {
            return left._x == right._x && left._y == right._y;
        }

        public static Boolean operator !=(TextLocation left, TextLocation right)
        {
            return !(left == right);
        }

        public static Boolean operator <(TextLocation left, TextLocation right)
        {
            if (left._x == right._x)
            {
                return left._y < right._y;
            }
            return left._x < right._x;
        }

        public static Boolean operator >(TextLocation left, TextLocation right)
        {
            if (left._x == right._x)
            {
                return left._y > right._y;
            }
            return left._x > right._x;
        }

        public static Boolean operator <=(TextLocation left, TextLocation right)
        {
            return !(left > right);
        }

        public static Boolean operator >=(TextLocation left, TextLocation right)
        {
            return !(left < right);
        }
        #endregion

        private Int32 _x;
        private Int32 _y;
    }
}
