using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    public sealed class StringTextSource : ITextSource
    {
        public StringTextSource(String source)
        {
            _source = source ?? throw new ArgumentNullException("source");
        }

        #region ITextSource
        public string Text => _source;

        public int Length => _source.Length;

        public char GetCharacterAt(int offset)
        {
            return _source[offset];
        }

        public string GetTextAt(int offset, int length)
        {
            return _source.Substring(offset, length);
        }

        public int IndexOfAny(char[] chars, int startIndex, int length)
        {
            return _source.IndexOfAny(chars, startIndex, length);
        }

        public int IndexOf(string content, int startIndex, int length)
        {
            return _source.IndexOf(content, startIndex, length);
        }

        public ICollection<int> AllIndexesOf(string content, int startIndex, int length)
        {
            var ret = new List<Int32>();
            Int32 idx = -1;
            while ((idx = _source.IndexOf(content, startIndex, length)) > 0)
            {
                ret.Add(idx);
                Int32 oldIdx = startIndex;
                startIndex = idx + length;
                length -= startIndex - oldIdx;
            }
            return ret;
        }

        public ITextSource CreateSnapshot()
        {
            return new StringTextSource(_source);
        }

        public TextReader CreateReader()
        {
            return new StringReader(_source);
        } 
        #endregion

        private String _source;
    }
}
