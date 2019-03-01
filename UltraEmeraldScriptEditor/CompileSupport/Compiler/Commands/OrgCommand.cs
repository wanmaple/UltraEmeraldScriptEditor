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
			throw new System.NotImplementedException();
		}

		public void AfterParse()
		{
			throw new System.NotImplementedException();
		}
	}
}