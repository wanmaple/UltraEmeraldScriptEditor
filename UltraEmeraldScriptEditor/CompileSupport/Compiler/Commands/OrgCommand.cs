namespace CompileSupport.Compiler.Commands
{
	public class OrgCommand:ExcutableCommand
	{
		public override void ToTempData(TempDataWriter writer)
		{
			writer._curOffset = parameters[0].IntValue;
		}
	}
	
	public class LableCommand:ExcutableCommand
	{
		public override void ToTempData(TempDataWriter writer)
		{
			keyword.intValue = writer._curOffset;
		}

		public override bool Match(Token first, CompilerApplication context)
		{
			TokenQueue tokens = context.context.Tokens;
			if (tokens.Peek().Text.Equals(":"))
			{
				tokens.Peek().Type = TokenType.LABEL;
				return true;
			}

			return false;
		}

		public override ExcutableCommand Create(TokenQueue seq, Token first, CompilerApplication context)
		{
			context.SetLabel(first.Text, first);
			LableCommand c = new LableCommand {keyword = first};
			return c;
		}
	}
}