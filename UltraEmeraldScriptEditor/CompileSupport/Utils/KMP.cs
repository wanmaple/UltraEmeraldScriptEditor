using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 单字符串的匹配算法。
    /// </summary>
    public class KMP
    {
        public KMP(String pattern)
        {
            _pattern = pattern ?? throw new ArgumentNullException("pattern");
            if (_pattern.Length == 0)
            {
                throw new ArgumentOutOfRangeException("pattern", "pattern's length must be positive.");
            }
            _pmt = GetPMT(pattern);
        }

        public Int32 Match(String text)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }

            return Match(text, 0, text.Length);
        }

        public Int32 Match(String text, Int32 startIndex, Int32 length)
        {
            if (text == null)
            {
                throw new ArgumentNullException("text");
            }
            VerifyRange(text, startIndex, length);
            if (text.Length <= 0)
            {
                return -1;
            }
            if (_pattern.Length > text.Length)
            {
                return -1;
            }

            /* 思想就是待匹配字符串的真后缀和已经匹配部分的真前缀存在最大公共部分，不需要重新逐个匹配
             * 
             *    ababababc           ababababc
             *                  =>   
             *    abababc               abababc
             *          ↑                   ↑
             * 最大公共部分:abab
             */
            Int32 maxPrefixLength = 0;
            for (int i = 0; i < text.Length; i++)
            {
                while (maxPrefixLength > 0 && text[i] != _pattern[maxPrefixLength])
                {
                    // 失配时找到pmt表里面上一个索引继续配对
                    maxPrefixLength = _pmt[maxPrefixLength - 1];
                }
                if (text[i] == _pattern[maxPrefixLength])
                {
                    ++maxPrefixLength;
                    if (maxPrefixLength == _pattern.Length)
                    {
                        return i - maxPrefixLength + 1;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// 部分匹配表(Partial Match Table)
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        private Int32[] GetPMT(String pattern)
        {
            Debug.Assert(pattern != null && pattern.Length > 0);
            var pmt = new Int32[pattern.Length];
            // 第一个前缀索引是0
            pmt[0] = 0;
            // 子串的遍历索引
            Int32 j = 0;
            for (int i = 1; i < pattern.Length; i++)
            {
                while (j > 0 && pattern[j] != pattern[i])
                {
                    // 不匹配的时候，因为已经匹配了j个字符，所以继续向前查看前缀是否匹配，直到发现仍然匹配的索引或者全部不匹配为止
                    j = pmt[j - 1];
                }
                if (pattern[j] == pattern[i])
                {
                    // 匹配，索引自增
                    ++j;
                }
                // 不匹配，记录已经匹配的长度
                pmt[i] = j;
            }
            return pmt;
        }

        private void VerifyRange(String text, Int32 index, Int32 length)
        {
            if (index < 0 || index >= text.Length)
            {
                throw new ArgumentOutOfRangeException("index", "0 <= index < " + text.Length.ToString());
            }
            if (index + length > text.Length)
            {
                throw new ArgumentOutOfRangeException("length");
            }
        }

        private Int32[] _pmt;
        private String _pattern;
    }
}
