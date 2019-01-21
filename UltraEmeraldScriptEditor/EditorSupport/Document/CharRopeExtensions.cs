using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// Rope<Char>的扩展方法，<see cref="Rope{T}"/>
    /// </summary>
    public static class CharRopeExtensions
    {
        public static String ToString(this Rope<Char> rope, Int32 index, Int32 length)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (length == 0)
            {
                return String.Empty;
            }
            Char[] buffer = new char[length];
            rope.CopyTo(index, buffer, 0, length);
            return new String(buffer);
        }

        public static void AppendText(this Rope<Char> rope, String text)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            rope.AddRange(text.ToArray());
        }

        public static void InsertText(this Rope<Char> rope, Int32 offset, String text)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            rope.InsertRange(offset, text.ToArray());
        }

        public static Int32 IndexOf(this Rope<Char> rope, String text)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (text.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("text");
            }
            return -1;
        }

        public static ICollection<Int32> IndexOfAll(this Rope<Char> rope, String text)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (text.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("text");
            }
            var ret = new List<Int32>();
            return ret;
        }
    }
}
