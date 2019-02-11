using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EditorSupport.Highlighting
{
    /// <summary>
    /// 使用关键字比对作为规则的制定器。
    /// </summary>
    public sealed class RegexHighlightRuler : IHighlightRuler
    {
        public Dictionary<String, Int32> RegexMap => _regexMap;
        public Dictionary<String, Int32> PrefixMap => _prefixMap;
        public List<Char> Splitters => _splitters;

        public RegexHighlightRuler()
        {
            _prefixMap = new Dictionary<string, int>();
            _regexMap = new Dictionary<string, int>();
            _splitters = new List<char>();
        }

        #region IHighlightRuler
        public void FormulateRule(IHighlightee highlightee)
        {
            String text = highlightee.Content.Trim();
            foreach (String expression in _regexMap.Keys)
            {
                Regex reg = new Regex(expression);
                if (reg.IsMatch(text))
                {
                    highlightee.HighlightRule = _regexMap[expression];
                    return;
                }
            }
            foreach (String prefix in _prefixMap.Keys)
            {
                if (text.StartsWith(prefix))
                {
                    highlightee.HighlightRule = _prefixMap[prefix];
                    return;
                }
            }
            highlightee.HighlightRule = -1;
        }

        public void SplitText(TextReader reader, Action<Int32, Int32> handler)
        {
            Int32 startIdx = 0;
            Boolean isSplitter = true;
            Int32 idx = 0;
            StringBuilder prefixSb = new StringBuilder();
            Int32 read;
            while ((read = reader.Read()) >= 0)
            {
                Char ch = (Char)read;
                if (idx == 0)
                {
                    isSplitter = _splitters.Contains(ch);
                }
                if (isSplitter)
                {
                    if (_splitters.Contains(ch))
                    {
                    }
                    else
                    {
                        isSplitter = false;
                        prefixSb.Clear();
                        prefixSb.Append(ch);

                        handler(startIdx, idx - startIdx);
                        startIdx = idx;
                    }
                }
                else
                {
                    if (!_splitters.Contains(ch))
                    {
                        prefixSb.Append(ch);
                    }
                    else
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
                        isSplitter = true;
                        prefixSb.Clear();

                        handler(startIdx, idx - startIdx);
                        startIdx = idx;
                    }
                }
                ++idx;
            }
            if (idx - startIdx > 0)
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
                handler(startIdx, idx - startIdx);
            }
        }
        #endregion

        private Dictionary<String, Int32> _regexMap;
        private Dictionary<String, Int32> _prefixMap;
        private List<Char> _splitters;
    }
}
