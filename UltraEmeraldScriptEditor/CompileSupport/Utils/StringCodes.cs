using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    public sealed class StringCodes
    {
        #region Singleton
        public static Char InvalidCharacter = '\0';

        public static StringCodes GetInstance()
        {
            if (_instance == null)
            {
                _instance = new StringCodes();
                using (var fs = new FileStream("Resources/code_translation.txt", FileMode.Open, FileAccess.Read))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        String line = null;
                        while (!String.IsNullOrEmpty(line = sr.ReadLine()))
                        {
                            var pair = line.Split('=');
                            UInt16 code = Convert.ToUInt16(pair[0], 16);
                            Char ch = pair[1][0];
                            _instance._encodeMap.Add(ch, code);
                            _instance._decodeMap.Add(code, ch);
                        }
                    }
                }
            }
            return _instance;
        }

        private StringCodes()
        {
            _encodeMap = new Dictionary<Char, UInt16>();
            _decodeMap = new Dictionary<UInt16, Char>();
        }

        private static StringCodes _instance; 
        #endregion
         
        public Byte[] Encode(Char ch)
        {
            if (_encodeMap.ContainsKey(ch))
            {
                UInt16 val = _encodeMap[ch];
                if (val <= Byte.MaxValue)
                {
                    return new Byte[] { (Byte)val, };
                }
                return BitConverter.GetBytes(val);
            }
            return null;
        }

        public Char Decode(UInt16 key)
        {
            if (_decodeMap.ContainsKey(key))
            {
                return _decodeMap[key];
            }
            return InvalidCharacter;
        }

        private IDictionary<UInt16, Char> _decodeMap;
        private IDictionary<Char, UInt16> _encodeMap;
    }
}
