using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 可以快速使用的轻量文本源，不保证线程安全。
    /// </summary>
    public sealed class RopeTextSource : ITextSource
    {
        public RopeTextSource(Rope<Char> rope)
        {
            _rope = rope ?? throw new ArgumentNullException("rope");
        }

        #region ITextSource
        public string Text
        {
            get { return _rope.ToString(0, _rope.Count); }
        }

        public int Length
        {
            get { return _rope.Count; }
        }

        public ICollection<int> AllIndexesOf(string content, int startIndex, int length)
        {
            return _rope.AllIndexesOfText(content, startIndex, length);
        }

        public TextReader CreateReader()
        {
            return new RopeTextReader(_rope.Clone() as Rope<Char>);
        }

        public ITextSource CreateSnapshot()
        {
            return new RopeTextSource(_rope.Clone() as Rope<Char>);
        }

        public char GetCharacterAt(int offset)
        {
            return _rope[offset];
        }

        public string GetTextAt(int offset, int length)
        {
            return _rope.ToString(offset, length);
        }

        public int IndexOf(string content, int startIndex, int length)
        {
            return _rope.IndexOfText(content, startIndex, length);
        }

        public int IndexOfAny(char[] chars, int startIndex, int length)
        {
            return _rope.IndexOfAny(chars, startIndex, length);
        }
        #endregion

        private readonly Rope<Char> _rope;
    }
}
