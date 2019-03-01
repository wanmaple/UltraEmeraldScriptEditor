using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler.Commands
{
	public class ByteCommand : ExcutableCommand
	{
		public override void ToTempData(TempDataWriter writer)
		{
			byte[] result = new byte[parameters.Length];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = (byte) parameters[i].IntValue;
			}

			writer.Write(result);
		}
	}
	
	public class HWordCommand : ExcutableCommand
	{

		public override unsafe void ToTempData(TempDataWriter writer)
		{
			byte[] result = new byte[parameters.Length * 2];
			fixed(void* ptr = result)
			{
				short* shortptr = (short *)ptr;
				for (int i = 0; i < result.Length; i++)
				{
					*shortptr++ = (short) parameters[i].IntValue;
				}
			}
			writer.Write(result);
		}
	}
	
	public class WordCommand : ExcutableCommand
	{
		public override unsafe void ToTempData(TempDataWriter writer)
		{
			byte[] result = new byte[parameters.Length * 4];
			fixed(void* ptr = result)
			{
				int* intptr = (int *)ptr;
				for (int i = 0; i < result.Length; i++)
				{
					*intptr++ =  parameters[i].IntValue;
				}
			}
			writer.Write(result);
		}
	}
}
