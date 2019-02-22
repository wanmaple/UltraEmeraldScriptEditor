using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Syntax.PScript
{
    /// <summary>
    /// 数值类型在解析的时候，如果看到负号，就解释为负数，否则统一解释为无符号数。
    /// </summary>
    public enum PScriptValueType
    {
        Byte = 0,
        HalfWord,
        Word,
        String,
    }
}
