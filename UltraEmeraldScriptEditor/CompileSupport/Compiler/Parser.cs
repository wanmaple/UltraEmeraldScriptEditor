using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using System.Text;
using CompileSupport.Compiler.Commands;

namespace CompileSupport.Compiler
{
	public class Parser
	{
		readonly CompilerApplication _application;
		readonly CompilerContext context;
		
		public Parser(CompilerApplication context)
		{
			_application = context;
			this.context = _application.context;
		}

		public void TryParse()
		{
			context.Tokenizer.Init(context.Tokens);
			ParseCommands();
			WriteTempDatas();
		}

		private void WriteTempDatas()
		{
			foreach (var cmd in context.Results)
			{
				cmd.ToTempData(context.TempData);
			}
		}

		private void ParseCommands()
		{
			int length = context.SystemCommands.Count;
			while (!context.Tokens.Empty())
			{
				Token t = context.Tokens.Dequeue();
				for (int i = 0; i < length; i++)
				{
					if (!context.SystemCommands[i].Match(t,_application)) continue;
					ExcutableCommand result = context.SystemCommands[i].Create(context.Tokens, t, _application);
					if(result != null) context.Results.Add(result);
					context.Tokens.DequeueSeparator();
					goto End;
				}
				//not matched
				throw new Exception("Not Matched Command:"+t);
				End:continue;
			}
		}

		public void Clean()
		{
			context.Clear();
		}
	}
}