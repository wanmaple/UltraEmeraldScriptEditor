using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// Rope<Char>的扩展方法。<see cref="Rope{T}"/>
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

        public static Int32 IndexOfAny(this Rope<Char> rope, Char[] chars, Int32 offset, Int32 length)
        {
            if (rope == null)
            {
                throw new ArgumentNullException("rope");
            }
            if (chars.Length <= 0)
            {
                return -1;
            }
            Int32 idx = 0;
            foreach (var node in rope.Leaves(rope._root))
            {
                if (idx + node.Length <= offset)
                {
                    idx += node.Length;
                }
                else
                {
                    Int32 relativeOffset = offset - idx;
                    if (relativeOffset >= 0)
                    {
                        idx += relativeOffset;
                    }
                    else
                    {
                        relativeOffset = 0;
                    }
                    for (int i = relativeOffset; i < node.Length; i++)
                    {
                        Char ch = node._contents[i];
                        if (chars.Contains(ch))
                        {
                            return idx;
                        }
                        ++idx;
                        if (idx > offset + length - 1)
                        {
                            return -1;
                        }
                    }
                }
            }
            return -1;
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
            foreach (var node in rope.Leaves(rope._root))
            {
                if (idx + node.Length <= offset)
                {
                    idx += node.Length;
                }
                else
                {
                    Int32 relativeOffset = offset - idx;
                    if (relativeOffset >= 0)
                    {
                        idx += relativeOffset;
                    }
                    else
                    {
                        relativeOffset = 0;
                    }
                    for (int i = relativeOffset; i < node.Length; i++)
                    {
                        Boolean found = true;
                        for (int j = 0; j < text.Length; j++)
                        {
                            Char ch = node._contents[i + j];
                            if (ch != text[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            return idx;
                        }
                        ++idx;
                        if (idx > offset + length - text.Length)
                        {
                            return -1;
                        }
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 该查找函数会寻找重复部分的字符
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
            foreach (var node in rope.Leaves(rope._root))
            {
                if (idx + node.Length <= offset)
                {
                    idx += node.Length;
                }
                else
                {
                    Int32 relativeOffset = offset - idx;
                    if (relativeOffset >= 0)
                    {
                        idx += relativeOffset;
                    }
                    else
                    {
                        relativeOffset = 0;
                    }
                    for (int i = relativeOffset; i < node.Length; i++)
                    {
                        Boolean found = true;
                        for (int j = 0; j < text.Length; j++)
                        {
                            Char ch = node._contents[i + j];
                            if (ch != text[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            foundIdxes.Add(idx);
                            idx += text.Length;
                        }
                        else
                        {
                            ++idx;
                        }
                        if (idx > offset + length - text.Length)
                        {
                            break;
                        }
                    }
                }
            }
            return foundIdxes;
        }
    }
}
