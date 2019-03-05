using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 用于字典中的双键。
    /// </summary>
    /// <remarks>
    /// 必要时需要重写GetHashCode()方法。
    /// </remarks>
    /// <typeparam name="K1"></typeparam>
    /// <typeparam name="K2"></typeparam>
    public interface IPairKey<K1, K2>
    {
        K1 Key1 { get; }
        K2 Key2 { get; }
    }
}
