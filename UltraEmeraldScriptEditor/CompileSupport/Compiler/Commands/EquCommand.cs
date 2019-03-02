using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace CompileSupport.Compiler.Commands
{
	public class EquCommand : Command,Interceptor
	{
		private EquTokenQueue replaced;
		
		public override ExcutableCommand Create(TokenQueue seq, Token first, ICompilerContext context)
		{
			string key = seq.Dequeue().Text;
			seq.Dequeue();
			Token value = seq.Dequeue();
			context.SetEqu(key, value);
			if (replaced != null) return null;
			EquTokenQueue queue = new EquTokenQueue(seq.Count, context);
			foreach (var c in seq)
			{
				queue.Enqueue(c);
			}
			seq.Clear();
			context.Tokens = queue;
			replaced = queue;
			return null;
		}

		public class EquTokenQueue : TokenQueue
		{
			public EquTokenQueue(int capacity, ICompilerContext _pool) : base(capacity)
			{
				this._pool = _pool;
			}

			internal ICompilerContext _pool;
			
			public override Token Dequeue()
			{
				Token t = base.Dequeue();
				if (t.Column > 1 && _pool.GetEqu(t.Text) != null) 
				{
					return _pool.GetEqu(t.Text);
				}
				return t;
			}
		}

		public void AfterParse()
		{
			replaced._pool = null;
		}
	}
}