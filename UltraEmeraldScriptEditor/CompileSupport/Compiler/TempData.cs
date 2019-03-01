using System.Collections.Generic;
using System.IO;

namespace CompileSupport.Compiler
{
	public struct TempData
	{
		public int offset;
		public byte[] data;
	}
	
	public class TempDataWriter:List<TempData>
	{
		public int _curOffset;

		public void Write(byte[] add)
		{
			Add(new TempData(){offset = _curOffset,data = add});
			_curOffset += add.Length;
		}

		public void WriteToSteam(BinaryWriter bw)
		{
			foreach (var data in this)
			{
				bw.Seek(data.offset, SeekOrigin.Begin);
				bw.Write(data.data);
			}
		}
	}
}