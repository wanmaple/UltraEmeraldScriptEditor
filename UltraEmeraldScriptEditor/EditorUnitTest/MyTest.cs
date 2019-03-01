using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CompileSupport.Compiler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EditorUnitTest
{
	[TestClass]
	public class MyTest
	{
		[TestMethod]
		public void test()
		{

			StringBuilder sb = new StringBuilder();
			sb.AppendLine(".equ sgv, 0x1");
			sb.AppendLine(".macro showsprite spriteid //commnet");
			sb.AppendLine(".byte 0x1 sgv \\spriteid 5 7 ");
			sb.AppendLine(".endm");
			sb.AppendLine(".byte sgv");
			sb.AppendLine("showsprite 0x1");
			sb.Append("//this is a comment");
			CompilerContext.SourceString = sb.ToString();
			Parser parser = new Parser(CompilerContext.SourceString);
			CompilerContext.Parser = parser;
			parser.TryParse();
			Debug.WriteLine(1);
		}
	}
}