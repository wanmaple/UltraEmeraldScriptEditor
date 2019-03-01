using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CompileSupport.Compiler
{
	public static class CompilerContext
	{
		public static Parser Parser { get; set; }
		
		public static string SourceString { get; set; }

		public static void Clean()
		{
			Parser = null;
			SourceString = null;
		}
	}

	public interface Interceptor
	{
		void AfterParse();
	}
}