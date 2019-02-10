using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace EditorSupport.Utils
{
    public static class CommonUtilities
    {
        public static Double Clamp(Double value, Double min, Double max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        /// <summary>
        /// 将十六进制字符串转成Color
        /// </summary>
        /// <param name="hex">必须是#ARGB格式</param>
        /// <returns></returns>
        public static Color ColorFromHexString(String hex)
        {
            if (String.IsNullOrEmpty(hex))
            {
                throw new ArgumentNullException("hex");
            }
            if (hex[0] != '#')
            {
                throw new ArgumentException("hex must start with '#'");
            }
            Byte[] argb = new Byte[4];
            for (int i = 0; i < 4; i++)
            {
                argb[i] = Convert.ToByte(hex.Substring(i * 2 + 1, 2), 16);
            }
            return Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
        }

        /// <summary>
        /// 将文本格式化。
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        /// <remarks>
        /// \t替换为空格，\n和\r替换为\r\n
        /// </remarks>
        public static String NormalizeText(TextReader reader)
        {
            StringBuilder sb = new StringBuilder();
            Int32 read;
            Nullable<Char> prev = null;
            while ((read = reader.Read()) >= 0)
            {
                Char ch = (Char)read;
                if (ch == '\t')
                {
                    // 改为4个空格
                    sb.Append("    ");
                }
                else if (ch == '\n')
                {
                    // \n统一改为\r\n
                    if (prev != null && prev != '\r')
                    {
                        sb.Append("\r");
                    }
                    sb.Append(ch);
                }
                else if (ch == '\r')
                {
                    // \r统一改为\r\n
                    sb.Append(ch);
                    Char next = (Char)reader.Peek();
                    if (next != '\n')
                    {
                        sb.Append('\n');
                    }
                }
                else
                {
                    sb.Append(ch);
                }
                prev = ch;
            }
            return sb.ToString();
        }
    }
}
