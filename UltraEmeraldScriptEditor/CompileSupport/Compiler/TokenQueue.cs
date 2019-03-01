using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler
{

	public class TokenQueue : Queue<Token>
	{
		public TokenQueue(int capacity):base(capacity){}
		
		public TokenQueue() {}
		
		public Token[] DeQueueLine()
		{
			if (tmp == null)
			{
				tmp = new List<Token>();
			}
			tmp.Clear();
			while (!Empty() && (Peek()).Type != TokenType.SEPARATOR)
			{
				tmp.Add(Dequeue());
			}
			DequeueSeparator();
			return tmp.ToArray();
		}

		public new virtual Token Dequeue()
		{
			return base.Dequeue();
		}

		public void DequeueSeparator()
		{
			while (!Empty() && Peek().Type == TokenType.SEPARATOR)
			{
				base.Dequeue();
			}
		}


		public bool Empty()
		{
			return Count == 0;
		}
		
		private List<Token> tmp;
	}
	
	
}
