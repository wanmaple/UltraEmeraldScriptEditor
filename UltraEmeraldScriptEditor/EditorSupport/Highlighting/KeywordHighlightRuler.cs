using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Highlighting
{
    /// <summary>
    /// 使用关键字比对作为规则的制定器。
    /// </summary>
    public sealed class KeywordHighlightRuler : IHighlightRuler
    {
        public Dictionary<String, Int32> KeywordMap => _keywordMap;
        public Dictionary<String, Int32> PrefixMap => _prefixMap;
        public Dictionary<String, Int32> StartMap => _startMap;
        public List<Char> Splitters => _splitters;

        public KeywordHighlightRuler()
        {
            _keywordMap = new Dictionary<string, int>();
            _prefixMap = new Dictionary<string, int>();
            _startMap = new Dictionary<string, int>();
            _splitters = new List<char>();
        }

        #region IHighlightRuler
        public void FormulateRule(IHighlightee highlightee)
        {
            String text = highlightee.Content.Trim();
            if (_keywordMap.ContainsKey(text))
            {
                highlightee.HighlightRule = _keywordMap[text];
            }
            else
            {
                foreach (String prefix in _prefixMap.Keys)
                {
                    if (text.StartsWith(prefix))
                    {
                        highlightee.HighlightRule = _prefixMap[prefix];
                        return;
                    }
                }
                foreach (String prefix in _startMap.Keys)
                {
                    if (text.StartsWith(prefix))
                    {
                        highlightee.HighlightRule = _startMap[prefix];
                        return;
                    }
                }
                highlightee.HighlightRule = -1;
            }
        }

        public void SplitText(TextReader reader, Action<Int32, Int32> handler)
        {
            Int32 startIdx = 0;
            Boolean spaceAtFirst = true;
            Int32 idx = 0;
            StringBuilder prefixSb = new StringBuilder();
            Int32 read;
            while ((read = reader.Read()) >= 0)
            {
                ++idx;
                Char ch = (Char)read;
                if (spaceAtFirst && _splitters.Contains(ch))
                {
                    continue;
                }
                else if (spaceAtFirst)
                {
                    spaceAtFirst = false;
                    prefixSb.Append(ch);
                    continue;
                }

                if (_splitters.Contains(ch))
                {
                    // 查看是否有符合的前缀
                    String prefixStr = prefixSb.ToString();
                    foreach (String prefix in _prefixMap.Keys)
                    {
                        if (prefixStr.StartsWith(prefix))
                        {
                            handler(startIdx, -1);
                            return;
                        }
                    }
                    prefixSb.Clear();

                    handler(startIdx, idx - startIdx);
                    startIdx = idx;
                    spaceAtFirst = true;
                }
                else
                {
                    prefixSb.Append(ch);
                }
            }
            handler(startIdx, idx - startIdx);
        }
        #endregion

        private Dictionary<String, Int32> _keywordMap;
        private Dictionary<String, Int32> _prefixMap;
        private Dictionary<String, Int32> _startMap;
        private List<Char> _splitters;
    }
}
