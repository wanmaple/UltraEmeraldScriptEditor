using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EditorSupport.Document
{
    /// <summary>
    /// 可读的文本对象。
    /// </summary>
    public interface ITextSource
    {
        /// <summary>
        /// 所有文本
        /// </summary>
        String Text { get; }
        /// <summary>
        /// 所有文本长度
        /// </summary>
        Int32 Length { get; }

        /// <summary>
        /// 获取偏移处的字符
        /// </summary>
        /// <param name="offset">文本偏移</param>
        /// <returns></returns>
        Char GetCharacterAt(Int32 offset);
        /// <summary>
        /// 获取偏移处某长度的文本
        /// </summary>
        /// <param name="offset">文本偏移</param>
        /// <param name="length">获取长度</param>
        /// <returns></returns>
        String GetTextAt(Int32 offset, Int32 length);
        /// <summary>
        /// 获取任意一个存在于chars内的字符的偏移
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Int32 IndexOfAny(Char[] chars, Int32 startIndex, Int32 length);
        /// <summary>
        /// 获取字符串的偏移
        /// </summary>
        /// <param name="anyOf">需要获取的字符列表</param>
        /// <param name="startIndex">搜索的起始偏移</param>
        /// <param name="length">搜索的长度</param>
        /// <returns></returns>
        Int32 IndexOf(String content, Int32 startIndex, Int32 length);
        /// <summary>
        /// 获取所有该字符串的偏移
        /// </summary>
        /// <param name="content"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        ICollection<Int32> AllIndexesOf(String content, Int32 startIndex, Int32 length);
        /// <summary>
        /// 获取一个快照
        /// </summary>
        /// <remarks>
        /// 不一定是线程安全的，如需要线程安全，请在实现中确保它
        /// </remarks>
        /// <returns></returns>
        ITextSource CreateSnapshot();
        /// <summary>
        /// 获取文本的TextReader对象
        /// </summary>
        /// <returns></returns>
        TextReader CreateReader();
    }
}
