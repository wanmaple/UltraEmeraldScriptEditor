using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Compiler
{
	public class Tokenizer
	{
		char[] sourceText;
		int pos;
		private int linenumber;
		private int columnnumber;

		public Tokenizer(char[] text)
		{
			sourceText = text;
		}

		public TokenQueue StartParse()
		{
			pos = 0;
			linenumber = 1;
			columnnumber = 1;
			return Init();
		}

		private void SkipWhiteSpace()
		{
			if (sourceText[pos] == ' ')
				pos++;
			if (sourceText[pos] == '/')
				SkipComment();
		}

		private void SkipComment() { if (sourceText[pos] == '/' && sourceText[pos + 1] == '/') EndLine(); }

		private void EndLine()
		{
			while (pos < sourceText.Length && sourceText[pos] != '\n')
			{
				pos++;
			}
			linenumber++;
			columnnumber = 1;
		}

		private TokenQueue Init()
		{
			TokenQueue tokens = new TokenQueue();
			while (pos < sourceText.Length) {
				SkipWhiteSpace();
				tokens.Enqueue(NexToken());
			}

			return tokens;
		}

		private Token NexToken()
		{
			int curpos = pos;
			if (curpos >= sourceText.Length) 
				return Token.SEPERATOR;
			switch (sourceText[curpos])//处理特殊符号开头
			{
				case '\r':
					columnnumber = 1;
					pos ++;
					if (sourceText[pos] == '\n') pos++;
					return Token.SEPERATOR;
				case '\n':
					pos++;
					columnnumber = 1; 
					return Token.SEPERATOR;
				case '.':
					curpos++;
					break;
				case '\\':
					curpos++;
					break;
			}
			if (Token.IsOperator(sourceText[curpos], out _)) curpos++;
			else while (curpos < sourceText.Length)
			{
				if (char.IsWhiteSpace(sourceText[curpos]) || char.IsPunctuation(sourceText[curpos]))
				{
					break;
				}
				curpos++;
			}
			if (curpos == pos) throw new Exception("符号不能以特殊符号开头");
			Token result =  new Token(sourceText, pos, curpos, linenumber, columnnumber);
			pos = curpos;
			columnnumber++;
			return result;
		}

		public void Clean()
		{
			sourceText = null;
		}
	}
}
