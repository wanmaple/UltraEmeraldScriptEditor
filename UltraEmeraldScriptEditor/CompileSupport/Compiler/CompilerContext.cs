using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
// ReSharper disable All

namespace CompileSupport.Compiler
{
	public class CompilerContext
	{
		public readonly Dictionary<string, Token> _equs = new Dictionary<string, Token>();
		public readonly Dictionary<string, Command> _macros = new Dictionary<string, Command>();
		public readonly Dictionary<string, Token> _lables = new Dictionary<string, Token>();

		public Tokenizer Tokenizer { get; }

		public TokenQueue Tokens { get; set; }

		public List<ExcutableCommand> Results { get; }

		public TempDataWriter TempData { get; }
		public List<Command> SystemCommands => _systemCommands;
		public List<Interceptor> Interceptors => _interceptors;

		private static readonly List<Command> _systemCommands = new List<Command>();

		private static readonly List<Interceptor> _interceptors = new List<Interceptor>();

		static CompilerContext()
		{
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			FieldInfo property = typeof(Command).GetField("keyword", BindingFlags.NonPublic | BindingFlags.Instance);
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

				if (tmp.BaseType != typeof(Command) || type.FullName == null || type.IsAbstract) continue;
				Command cmd = (Command) type.Assembly.CreateInstance(type.FullName);
				property.SetValue(cmd, new Token(string.Concat(".",
					type.Name.Substring(0, type.Name.Length - "command".Length).ToLower()), TokenType.COMMAND));
				_systemCommands.Add(cmd);
				if (type.GetInterface(typeof(Interceptor).FullName) != null)
				{
					_interceptors.Add((Interceptor) cmd);
				}
			}
		}


		public CompilerContext(string source)
		{
			Tokenizer = new Tokenizer(source.ToCharArray());
			Tokens = new TokenQueue();
			TempData = new TempDataWriter();
			Results = new List<ExcutableCommand>();
		}

		

		public void Clear()
		{
			_macros.Clear();
			_equs.Clear();
			_lables.Clear();
		}
	}

	public class CompilerApplication
	{
		public CompilerContext context { get; set; }

		public CompilerApplication(string str)
		{
			context = new CompilerContext(str);
		}
		
		public void SetEqu(string key, Token value)
		{
			context._equs[key] = value;
		}

		public Token GetEqu(string key)
		{
			context._equs.TryGetValue(key, out Token v);
			return v;
		}

		public void SetMacro(string key, Command value)
		{
			context._macros[key] = value;
		}

		public Command GetMacro(string key)
		{
			context._macros.TryGetValue(key, out Command value);
			return value;
		}

		public void SetLabel(string key, Token value)
		{
			context._lables[key] = value;
		}

		public Token GetLabel(string key)
		{
			context._lables.TryGetValue(key, out Token t);
			return t;
		}
	}

	

	public interface Interceptor
	{
		void AfterParse();
	}
}