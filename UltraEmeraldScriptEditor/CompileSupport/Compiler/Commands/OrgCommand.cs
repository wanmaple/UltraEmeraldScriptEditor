namespace CompileSupport.Compiler.Commands
{
	public class OrgCommand:ExcutableCommand
	{
		public override void ToTempData(TempDataWriter writer)
		{
			writer._curOffset = parameters[0].IntValue;
		}
	}
	
	public class LableCommand:ExcutableCommand,Interceptor
	{
		public override void ToTempData(TempDataWriter writer)
		{
			keyword.intValue = writer._curOffset;
		}

		public override bool Match(Token first, ICompilerContext context)
		{
			TokenQueue tokens = context.Tokens;
			if (tokens.Peek().Text.Equals(":"))
			{
				tokens.Peek().Type = TokenType.LABEL;
				return true;
			}

			return false;
		}

		public void AfterParse()
		{
			throw new System.NotImplementedException();
		}
		
	}
}