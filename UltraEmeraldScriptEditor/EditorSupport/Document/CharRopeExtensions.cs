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

        public static Int32 IndexOfText(this Rope<Char> rope, String text, Int32 offset, Int32 length)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (text.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("text");
            }
            Int32 idx = 0;
            Int32 foundIdx = -1;
            Int32 textIdx = 0;
            foreach (var node in rope.Leaves(rope._root))
            {
                if (idx > rope.Count - text.Length)
                {
                    break;
                }
                if (node.Length > 0)
                {
                    for (int i = 0; i < node.Length; i++)
                    {
                        if (idx >= offset && idx < offset + length)
                        {
                            Char ch = node._contents[i];
                            if (ch == text[textIdx])
                            {
                                if (foundIdx < 0)
                                {
                                    foundIdx = idx;
                                }
                                ++textIdx;
                            }
                            else
                            {
                                foundIdx = -1;
                                textIdx = 0;
                            }
                            if (textIdx >= text.Length)
                            {
                                return foundIdx;
                            }
                        }
                        ++idx;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 该查找函数不会寻找重复部分的字符
        /// 比如在"abcabcabc"中查找"abcabc"则只会返回0
        /// </summary>
        /// <param name="rope"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static ICollection<Int32> AllIndexesOfText(this Rope<Char> rope, String text, Int32 offset, Int32 length)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (text.Length <= 0)
            {
                throw new ArgumentOutOfRangeException("text");
            }
            var foundIdxes = new List<Int32>();
            Int32 idx = 0;
            Int32 textIdx = 0;
            Int32 foundIdx = -1;
            foreach (var node in rope.Leaves(rope._root))
            {
                if (idx > rope.Count - text.Length)
                {
                    break;
                }
                if (node.Length > 0)
                {
                    for (int i = 0; i < node.Length; i++)
                    {
                        if (idx >= offset && idx < offset + length)
                        {
                            Char ch = node._contents[i];
                            if (ch == text[textIdx])
                            {
                                if (foundIdx < 0)
                                {
                                    foundIdx = idx;
                                }
                                ++textIdx;
                            }
                            else
                            {
                                foundIdx = -1;
                                textIdx = 0;
                            }
                            if (textIdx >= text.Length)
                            {
                                foundIdxes.Add(foundIdx);
                                foundIdx = -1;
                                textIdx = 0;
                            }
                        }
                        ++idx;
                    }
                }
            }
            return foundIdxes;
        }
    }
}
