using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 使用逆波兰(Reverse Polish Notation)算法的计算器（整数）。
    /// </summary>
    /// <remarks>
    /// 将中缀表达式转换为后缀表达式
    /// 1+2-3 => 12+3-
    /// 1+2*3 => 123*+
    /// (1+2)*3 => 12+3*
    /// 4*(1+2-3)/5 => 412+3-*5/
    /// (1+2*3)+4*5 => 123*+45*+
    /// </remarks>
    public class RPNIntegerCalculator
    {
        /// <summary>
        /// 括号，仅仅用于标记。
        /// </summary>
        private class Parenthese : IIntegerBinaryOperator
        {
            public Parenthese(Char ch)
            {
                _ch = ch;
            }

            public int Priority => throw new NotImplementedException();

            public Regex OperatorRegex => throw new NotImplementedException();

            public int Calculate(int left, int right)
            {
                throw new NotImplementedException();
            }

            private Char _ch;
        }

        private static readonly Parenthese LeftParenthese = new Parenthese('(');
        private static readonly Parenthese RightParenthese = new Parenthese(')');

        public List<IIntegerBinaryOperator> Operators => _operators;

        public RPNIntegerCalculator()
        {
            _operators = new List<IIntegerBinaryOperator>();
            _operators.Add(new AdditionOperator());
            _operators.Add(new SubstractionOperator());
            _operators.Add(new MultiplicationOperator());
            _operators.Add(new DivisionOperator());
            _operators.Add(new BitAndOperator());
            _operators.Add(new BitOrOperator());
            _operators.Add(new BitXorOperator());
            _operators.Add(new BitShiftLeftOperator());
            _operators.Add(new BitShiftRightOperator());
            _operatorStack = new Stack<IIntegerBinaryOperator>();
            _operandStack = new Stack<int>();
            _regexDec = new Regex(@"[-+]?\d+");
            _regexHex = new Regex(@"[-+]?0[xX][0-9a-fA-F]+");
        }

        public void Reset()
        {
            _operatorStack.Clear();
            _operandStack.Clear();
        }

        public Int32 Calculate(String source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            Reset();
            // 清除所有空格
            String str = source.Replace(" ", String.Empty);
            // 第一个元素一定是操作数
            Int32 startAt = 0;
            Int32 operand;
            IIntegerBinaryOperator @operator;
            Boolean operandLast = false;
            while (startAt < str.Length)
            {
                if (operandLast)
                {
                    // 操作数后必须跟操作符
                    @operator = MatchOperator(str, ref startAt);
                    if (@operator == RightParenthese)
                    {
                        // 如果是右括号，则出栈直到遇到左括号
                        if (_operatorStack.Count == 0 || _operatorStack.Peek() == LeftParenthese || _operandStack.Count < 2)
                        {
                            ThrowInvalid(source);
                        }
                        while ((@operator = _operatorStack.Pop()) != LeftParenthese)
                        {
                            Int32 right = _operandStack.Pop();
                            Int32 left = _operandStack.Pop();
                            Int32 newOperand = @operator.Calculate(left, right);
                            _operandStack.Push(newOperand);
                            if (_operatorStack.Count == 0)
                            {
                                ThrowInvalid(source);
                            }
                        }
                    }
                    else if (_operatorStack.Count == 0 || @operator == LeftParenthese || _operatorStack.Peek() == LeftParenthese || @operator.Priority > _operatorStack.Peek().Priority)
                    {
                        // 优先级更高的运算符直接入栈
                        _operatorStack.Push(@operator);
                        operandLast = false;
                    }
                    else
                    {
                        IIntegerBinaryOperator temp = @operator;
                        // 运算符优先级<=栈顶运算符优先级，则运算到空栈为止
                        while (_operatorStack.Count > 0 && _operatorStack.Peek() != LeftParenthese)
                        {
                            @operator=_operatorStack.Pop();
                            if (@operator == LeftParenthese || @operator == RightParenthese)
                            {
                                ThrowInvalid(source);
                            }
                            Int32 right = _operandStack.Pop();
                            Int32 left = _operandStack.Pop();
                            Int32 newOperand = @operator.Calculate(left, right);
                            _operandStack.Push(newOperand);
                        }
                        _operatorStack.Push(temp);
                        operandLast = false;
                    }
                }
                else
                {
                    try
                    {
                        operand = MatchOperand(str, ref startAt);
                        _operandStack.Push(operand);
                        operandLast = true;
                    }
                    catch (FormatException)
                    {
                        @operator = MatchOperator(str, ref startAt);
                        if (@operator != LeftParenthese)
                        {
                            ThrowInvalid(source);
                        }
                        _operatorStack.Push(@operator);
                        operandLast = false;
                    }
                }
            }
            // 运算到空栈即为结果
            while (_operatorStack.Count > 0)
            {
                @operator = _operatorStack.Pop();
                if (@operator == LeftParenthese || @operator == RightParenthese)
                {
                    ThrowInvalid(source);
                }
                Int32 right = _operandStack.Pop();
                Int32 left = _operandStack.Pop();
                Int32 newOperand = @operator.Calculate(left, right);
                _operandStack.Push(newOperand);
            }
            return _operandStack.Pop();
        }

        private Int32 MatchOperand(String source, ref Int32 startAt)
        {
            Match match = _regexHex.Match(source, startAt);
            if (match.Length == 0 || match.Index != startAt)
            {
                match = _regexDec.Match(source, startAt);
            }
            if (match.Length == 0 || match.Index != startAt)
            {
                ThrowInvalid(source);
            }
            Int32 ret;
            if (!Int32.TryParse(match.Value, out ret))
            {
                ret = Convert.ToInt32(match.Value, 16);
            }
            startAt += match.Value.Length;
            return ret;
        }

        private IIntegerBinaryOperator MatchOperator(String source, ref Int32 startAt)
        {
            if (source[startAt] == '(')
            {
                ++startAt;
                return LeftParenthese;
            }
            else if (source[startAt] == ')')
            {
                ++startAt;
                return RightParenthese;
            }
            Match match = null;
            foreach (var op in _operators)
            {
                match = op.OperatorRegex.Match(source, startAt);
                if (match.Length > 0 && match.Index == startAt)
                {
                    startAt += match.Value.Length;
                    return op;
                }
            }
            ThrowInvalid(source);
            return null;
        }

        private void ThrowInvalid(String source)
        {
            throw new FormatException(String.Format("Cannot recognize '{0}'.", source));
        }

        private List<IIntegerBinaryOperator> _operators;
        private Stack<IIntegerBinaryOperator> _operatorStack;
        private Stack<Int32> _operandStack;
        private Regex _regexDec;
        private Regex _regexHex;
    }

    /// <summary>
    /// 二目运算符
    /// </summary>
    public interface IIntegerBinaryOperator
    {
        Int32 Priority { get; }
        Regex OperatorRegex { get; }
        
        Int32 Calculate(Int32 left, Int32 right);
    }

    public class AdditionOperator : IIntegerBinaryOperator
    {
        public int Priority => 1;

        public Regex OperatorRegex => new Regex("\\+");

        public int Calculate(int left, int right)
        {
            return left + right;
        }
    }

    public class SubstractionOperator : IIntegerBinaryOperator
    {
        public int Priority => 1;

        public Regex OperatorRegex => new Regex("\\-");

        public int Calculate(int left, int right)
        {
            return left - right;
        }
    }

    public class MultiplicationOperator : IIntegerBinaryOperator
    {
        public int Priority => 2;

        public Regex OperatorRegex => new Regex("\\*");

        public int Calculate(int left, int right)
        {
            return left * right;
        }
    }

    public class DivisionOperator : IIntegerBinaryOperator
    {
        public int Priority => 2;

        public Regex OperatorRegex => new Regex("/");

        public int Calculate(int left, int right)
        {
            return left / right;
        }
    }

    public class BitAndOperator : IIntegerBinaryOperator
    {
        public int Priority => 0;

        public Regex OperatorRegex => new Regex("&");

        public int Calculate(int left, int right)
        {
            return left & right;
        }
    }

    public class BitOrOperator : IIntegerBinaryOperator
    {
        public int Priority => 0;

        public Regex OperatorRegex => new Regex("\\|");

        public int Calculate(int left, int right)
        {
            return left | right;
        }
    }

    public class BitXorOperator : IIntegerBinaryOperator
    {
        public int Priority => 0;

        public Regex OperatorRegex => new Regex("\\^");

        public int Calculate(int left, int right)
        {
            return left ^ right;
        }
    }

    public class BitShiftLeftOperator : IIntegerBinaryOperator
    {
        public int Priority => 0;

        public Regex OperatorRegex => new Regex("<<");

        public int Calculate(int left, int right)
        {
            return left << right;
        }
    }

    public class BitShiftRightOperator : IIntegerBinaryOperator
    {
        public int Priority => 0;

        public Regex OperatorRegex => new Regex(">>");

        public int Calculate(int left, int right)
        {
            return left >> right;
        }
    }
}
