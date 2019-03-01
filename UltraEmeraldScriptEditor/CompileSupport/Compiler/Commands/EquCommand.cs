using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace CompileSupport.Compiler.Commands
{
	public class EquCommand : Command,Interceptor
	{
		private static readonly Dictionary<string, Token> _Pool = new Dictionary<string, Token>();

		private EquTokenQueue replaced;
		
		public override ExcutableCommand Create(TokenQueue seq, Token first)
		{
			string key = seq.Dequeue().Text;
			seq.Dequeue();
			Token value = seq.Dequeue();
			_Pool[key] = value;
			if (replaced != null) return null;
			EquTokenQueue queue = new EquTokenQueue(seq.Count);
			foreach (var c in seq)
			{
				queue.Enqueue(c);
			}
			seq.Clear();
			CompilerContext.Parser.Tokens = queue;
			replaced = queue;
			return null;
		}

		public class EquTokenQueue : TokenQueue
		{
			public EquTokenQueue(int capacity):base(capacity){}
			
			public override Token Dequeue()
			{
				Token t = base.Dequeue();
				if (t.Column > 1 && _Pool.TryGetValue(t.Text, out Token value))
				{
					t.Text = value.Text;
					t.Type = value.Type;
					t.intValue = value.intValue;
				}
				return t;
			}
		}

		public void AfterParse()
		{
			_Pool.Clear();
		}
	}
}