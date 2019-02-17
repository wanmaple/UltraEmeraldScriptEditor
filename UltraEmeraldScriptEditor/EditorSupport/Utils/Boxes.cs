using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EditorSupport.Utils
{
    /// <summary>
    /// 缓存装箱的值类型，避免反复装箱。
    /// </summary>
    public static class Boxes
    {
        public static readonly Object True = true;
        public static readonly Object False = false;
    }
}
