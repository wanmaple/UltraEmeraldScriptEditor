using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CompileSupport.Compiler
{
	public class Compiler : ICompilerContext
	{
		private readonly Dictionary<string, Token> _equs = new Dictionary<string, Token>();
		private readonly Dictionary<string, Command> _macros = new Dictionary<string, Command>();
		private readonly Dictionary<string, Token> _lables = new Dictionary<string, Token>();

		public Tokenizer Tokenizer { get; }

		public TokenQueue Tokens { get; set; }

		public List<ExcutableCommand> Results { get; }

		public TempDataWriter TempData { get; }
		public List<Command> SystemCommands => _systemCommands;
		public List<Interceptor> Interceptors => _interceptors;

		private static readonly List<Command> _systemCommands = new List<Command>();

		private static readonly List<Interceptor> _interceptors = new List<Interceptor>();

		static Compiler()
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


		public Compiler(string source)
		{
			Tokenizer = new Tokenizer(source.ToCharArray());
			Tokens = new TokenQueue();
			TempData = new TempDataWriter();
			Results = new List<ExcutableCommand>();
		}

		public void SetEqu(string key, Token value)
		{
			_equs[key] = value;
		}

		public Token GetEqu(string key)
		{
			return _equs[key];
		}

		public Dictionary<string, Token> Equs => _equs;

		public void SetMacro(string key, Command value)
		{
			_macros[key] = value;
		}

		public Command GetMacro(string key)
		{
			return _macros[key];
		}

		public void SetLabel(string key, Token value)
		{
			_lables[key] = value;
		}

		public Token GetLabel(string key)
		{
			return _lables[key];
		}

		public void Clear()
		{
			_macros.Clear();
			_equs.Clear();
			_lables.Clear();
		}
	}

	public interface ICompilerContext
	{
		void SetEqu(string key, Token value);

		Token GetEqu(string key);
		
		void SetMacro(string key, Command value);

		Command GetMacro(string key);

		void SetLabel(string key, Token value);

		Token GetLabel(string key);

		void Clear();

		Tokenizer Tokenizer { get; }

		TokenQueue Tokens { get; set; }

		List<ExcutableCommand> Results { get; }

		TempDataWriter TempData { get; }

		List<Command> SystemCommands { get; }
		
		List<Interceptor> Interceptors { get; }
	}

	public interface Interceptor
	{
		void AfterParse();
	}
}