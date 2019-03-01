using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using CompileSupport.Compiler.Commands;

namespace CompileSupport.Compiler
{
	public class Parser
	{
		public Tokenizer Tokenizer { get; }

		public TokenQueue Tokens { get; set; }

		public List<ExcutableCommand> Results { get; private set; }
		
		public TempDataWriter TempData { get; private set; }

		private static readonly List<Command> systemCommands = new List<Command>();
		
		private static readonly List<Interceptor> interceptors = new List<Interceptor>();

		static Parser()
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			FieldInfo  property = typeof(Command).GetField("keyword", BindingFlags.NonPublic|BindingFlags.Instance);
			Debug.Assert(property != null, nameof(property) + " != null");
			foreach (var type in types)
			{
				Type tmp = type;
				while (tmp.BaseType != null)
				{
					if (tmp.BaseType == typeof(Command))
					{
						break;
					}
					tmp = tmp.BaseType;
				}
				if(tmp.BaseType != typeof(Command) || type.FullName == null || type.IsAbstract) continue;
				Command cmd = (Command) type.Assembly.CreateInstance(type.FullName);
				property.SetValue(cmd, new Token(string.Concat(".",
					type.Name.Substring(0,type.Name.Length - "command".Length).ToLower()), TokenType.COMMAND));
				systemCommands.Add(cmd);
				if (type.GetInterface(typeof(Interceptor).FullName) != null)
				{
					interceptors.Add((Interceptor) cmd);
				}
			}
		}

		public Parser(string source)
		{
			Tokenizer = new Tokenizer(source.ToCharArray());
		}

		public void TryParse()
		{
			Tokens = Tokenizer.StartParse();
			ParseCommands();
			WriteTempDatas();
		}

		private void WriteTempDatas()
		{
			TempData = new TempDataWriter();
			foreach (var cmd in Results)
			{
				cmd.ToTempData(TempData);
			}
		}

		private void ParseCommands()
		{
			Results = new List<ExcutableCommand>();
			int length = systemCommands.Count;
			while (!Tokens.Empty())
			{
				Token t = Tokens.Dequeue();
				for (int i = 0; i < length; i++)
				{
					if (!systemCommands[i].Match(t)) continue;
					ExcutableCommand result = systemCommands[i].Create(Tokens, t);
					if(result != null) Results.Add(result);
					Tokens.DequeueSeparator();
					goto End;
				}
				//not matched
				throw new Exception("Not Matched Command:"+t);
				End:continue;
			}
		}

		public void Clean()
		{
			Tokens.Clear();
			Results.Clear();
			Tokenizer.Clean();
			foreach (var interceptor in interceptors)
			{
				interceptor.AfterParse();
			}
		}
	}
}