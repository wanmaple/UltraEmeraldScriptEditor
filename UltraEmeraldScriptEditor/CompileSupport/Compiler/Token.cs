using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CompileSupport.Utils;

namespace CompileSupport.Compiler
{
	public enum TokenType : byte
	{
		NONE,
		SEPARATOR,
		INVALID,
		NUMBER,
		WORD,
		OPERATOR,
		PARAMETER,
		PARAMETER_INDEX,
		COMMAND,
		MACRO_COMMAND,
		LABEL
	}

	public class Token
	{
		public int intValue;
		public int Line { get; }
		public int Column { get; }
		public TokenType Type { get; set; }

		public string Text { get; }

		public int IntValue
		{
			get
			{
				if (Type == TokenType.NUMBER || Type == TokenType.PARAMETER_INDEX) 
					return intValue;
				return Convert.ToInt32(Line);
			}
			set => intValue = value;
		}

		public static Token SEPERATOR = new Token("separator", TokenType.SEPARATOR);

		private static readonly string[] Operators = {"+", "-", "*", "/", "&", "|", "^"};

		public static bool IsOperator(char ch, out int i)
		{
			i = 0;
			if (char.IsPunctuation(ch)) return true;
			for (; i < Operators.Length; i++) {
				if (Operators[i][0] != ch) continue;
				return true;
			}
			return false;
		}

		public Token(char[] source, int start, int end, int line ,int column)
		{
			int length = end - start;
			if (length == 0) return;
			Line = line;
			Column = column;
			if (length == 1)
			{
				if(IsOperator(source[start],out int index)) {
					Text = Operators[index];
					Type = TokenType.OPERATOR;
					return;
				}
			}
			Text = new string(source, start, end - start);
			CheckOtherTypes();
		}
		
		
		public Token(string source, TokenType type){ 
			Text = source;
			Type = type;
		}

		public override string ToString()
		{
			return Text;
		}

		private bool IsWord(int start)
		{
			if (char.IsDigit(Text[start]))
			{
				return false;
			}
			for (var i = start + 1; i < Text.Length; i++)
			{
				if (!(char.IsLetterOrDigit(Text[i]) || Text[i] == '_')) return false;
			}

			return true;
		}

		private void CheckOtherTypes()
		{
			Type = TokenType.INVALID;
			char first = Text[0];
			bool isAddorSub = first == '-' || first == '+';
			if (char.IsNumber(first) || isAddorSub  && Text.Length > 1)
			{
				int firstindex = 0;
				if (isAddorSub) firstindex++;
				if(Text[firstindex] == '0' && Text.Length > firstindex + 2 && char.ToLower(Text[firstindex + 1]) == 'x' 
				&& int.TryParse(Text.Replace("0x", ""), 
					   NumberStyles.HexNumber,NumberFormatInfo.CurrentInfo,out intValue))
				{
					Type = TokenType.NUMBER;
				}
				else if (int.TryParse(Text, out intValue))
				{
					Type = TokenType.NUMBER;
				}
			}
			else if(first == '\\' && IsWord(1))
			{
				Type = TokenType.PARAMETER;
			}
			else if (first == '.' && IsWord(1))
			{
				Type = TokenType.COMMAND;
			}else if (IsWord(0))
			{
				Type = TokenType.WORD;
			}
		}

	public override bool Equals(object obj)
		{

			if (obj == null || GetType() != obj.GetType()) {
				return false;
			}

			return Text.Equals(((Token)obj).Text);
		}

		public override int GetHashCode()
		{
			return Text.GetHashCode();
		}
	}
}
