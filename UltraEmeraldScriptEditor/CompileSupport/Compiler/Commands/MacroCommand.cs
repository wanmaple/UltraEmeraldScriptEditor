using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler.Commands
{
	public class MacroCommand : ExcutableCommand
	{

		internal ExcutableCommand[] content;

		public override bool Match(Token first, ICompilerContext context)
		{
			if (base.Match(first, context) || context.GetMacro(first.Text) != null)
			{
				return true;
			}
			return false;
		}

		public override ExcutableCommand Create(TokenQueue seq, Token first, ICompilerContext context)
		{
			if (base.Match(first,context))
			{
				MacroCommand c = (MacroCommand) Clone();
				c.keyword = seq.Dequeue();
				c.keyword.Type = TokenType.MACRO_COMMAND;
				c.parameters = seq.DeQueueLine();
				context.SetMacro(c.keyword.Text, c);
				return c;
			}
			return (MacroCommand) Clone(seq, context.GetMacro(first.Text));
		}

		internal void CompileMacro()
		{
			foreach (var cmd in content)
			{
				foreach (var param in cmd.parameters)
				{
					if (param.Type == TokenType.PARAMETER)
					{
						param.IntValue = Find(parameters, param);
						param.Type = TokenType.PARAMETER_INDEX;
					}
					
				}
			}
		}

		private static int Find(Token[] parameters, Token param)
		{
			for (int i = 0; i < parameters.Length; i++)
			{
				if (param.Text.EndsWith(parameters[i].Text))
				{
					return i;
				}
			}
			throw new IndexOutOfRangeException();
		}

		public override void ToTempData(TempDataWriter writer)
		{
			foreach (var cmd in content)
			{
				for (int i = 0; i < cmd.parameters.Length; i++)
				{
					if (cmd.parameters[i].Type == TokenType.PARAMETER_INDEX)
						cmd.parameters[i].intValue = parameters[cmd.parameters[i].intValue].intValue;
				}
				cmd.ToTempData(writer);
			}
			
		}
	}


	public class EndmCommand : Command
	{
		public override ExcutableCommand Create(TokenQueue seq, Token first, ICompilerContext context)
		{
			List<ExcutableCommand> result = context.Results;
			int i;
			for (i = result.Count - 1; i >= 0; i--)
			{
				if (result[i].keyword.Type == TokenType.MACRO_COMMAND)
				{
					break;
				}
			}
			ExcutableCommand[] param = new ExcutableCommand[result.Count - i - 1];
			for (int j = param.Length - 1,k = result.Count - 1; j >= 0; j--,k--)
			{
				
				param[j] = result[k];
				result.RemoveAt(k);
			}

			MacroCommand macro = (MacroCommand) result[i];
			macro.content = param;
			result.RemoveAt(i);
			macro.CompileMacro();
			return null;
		}
	}
}