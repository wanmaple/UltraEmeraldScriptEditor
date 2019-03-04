using System;
using CompileSupport.Compiler.Commands;


namespace CompileSupport.Compiler
{
	public class Command : ICloneable
	{
		protected internal Token keyword;
		
		protected internal Token[] parameters;
		
		public object Clone()
		{
			return MemberwiseClone();
		}
		
		public Command(){}

		public virtual ExcutableCommand Create(TokenQueue seq, Token first, CompilerApplication context)
		{
			 return Clone(seq, this);
		}

		protected static ExcutableCommand Clone(TokenQueue seq, Command r)
		{
			r =  (Command) r.Clone();
			r.parameters = seq.DeQueueLine();
			return (ExcutableCommand) r;
		}
		
		public virtual bool Match(Token first, CompilerApplication context)
		{
			if (keyword.Equals(first))
			{
				return true;
			}
			return false;
		}
	}

	public abstract class ExcutableCommand : Command
	{
		public abstract void ToTempData(TempDataWriter writer);
	}
}
