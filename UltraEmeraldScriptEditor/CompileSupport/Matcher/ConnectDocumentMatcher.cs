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
        public List<Regex> FullyMatchRegexList => _fullyMatchRegexList;
        public List<Regex> ConnectRegexList => _connectRegexList;

        #region ConnectDocumentMatcher
        public ConnectDocumentMatcher(ITextSource source)
        {
            _source = source ?? throw new ArgumentNullException("source");
            _currentOffset = 0;
            _fullyMatchRegexList = new List<Regex>();
            _connectRegexList = new List<Regex>();
        } 
        #endregion

        #region IDocumentMatcher
        public ITextSource Document => _source;

        public int CurrentOffset { get => _currentOffset; set => _currentOffset = value; }

        public ISegment MatchFrom(int startOffset)
        {
            if (startOffset < 0 || startOffset > _source.Length)
            {
                return null;
            }
            _currentOffset = startOffset;
            return MatchNext();
        }

        public ISegment MatchNext()
        {
            throw new NotImplementedException();
        }
        #endregion

        private ITextSource _source;
        private Int32 _currentOffset;
        private ISegment _nextSegement;
        private List<Regex> _fullyMatchRegexList;
        private List<Regex> _connectRegexList;
    }
}
