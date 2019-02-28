using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EditorSupport.Document;

namespace CompileSupport.Matcher
{
    public sealed class ConnectDocumentMatcher : IDocumentMatcher
    {
        public List<Regex> PrimaryRegexList => _primaryRegexList;
        public List<Regex> SecondaryRegexList => _secondaryRegexList;
        public List<Regex> ConnectRegexList => _connectRegexList;

        #region ConnectDocumentMatcher
        public ConnectDocumentMatcher(ITextSource source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _content = _source.Text;
            _currentOffset = 0;
            _primaryRegexList = new List<Regex>();
            _connectRegexList = new List<Regex>();
            _nextSegment = null;
        }
        #endregion

        #region IDocumentMatcher
        public ITextSource Document => _source;

        public int CurrentOffset { get => _currentOffset; set => _currentOffset = value; }

        public ISegment MatchFrom(int startOffset)
        {
            if (startOffset < 0 || startOffset > _source.Length)
            {
                throw new ArgumentOutOfRangeException("startOffset");
            }
            _currentOffset = startOffset;
            return MatchNext();
        }

        public ISegment MatchNext()
        {
            Int32 matchOffset = _currentOffset;
            // 先过滤空格和换行符
            while (_content[matchOffset] == ' ' || _content[matchOffset] == '\r' || _content[matchOffset] == '\n')
            {
                ++matchOffset;
            }
            Match match = null;
            ISegment ret = null;
            foreach (var regex in _primaryRegexList)
            {
                match = regex.Match(_content, matchOffset);
                if (match.Length > 0 && match.Index == matchOffset)
                {
                    ret = new SimpleSegment(match.Index, match.Length);
                    matchOffset += match.Length;
                    return ret;
                }
            }
            foreach (var regex in _secondaryRegexList)
            {
                match = regex.Match(_content, matchOffset);
                if (match.Length > 0 && match.Index == matchOffset)
                {
                    ret = new SimpleSegment(match.Index, match.Length);
                    matchOffset += match.Length;
                    return ret;
                }
            }
            return ret;
        }
        #endregion

        private ITextSource _source;
        private String _content;
        private Int32 _currentOffset;
        private ISegment _nextSegment;
        private List<Regex> _primaryRegexList;
        private List<Regex> _secondaryRegexList;
        private List<Regex> _connectRegexList;
    }
}
