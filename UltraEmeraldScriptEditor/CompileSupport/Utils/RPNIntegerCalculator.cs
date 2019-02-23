using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 使用逆波兰(Reverse Polish Notation)算法的计算器（整数）。
    /// </summary>
    public class RPNIntegerCalculator
    {
        public List<IIntegerBinaryOperator> Operators => _operators;

        public RPNIntegerCalculator()
        {
            _operators = new List<IIntegerBinaryOperator>();
        }

        public Int32 Calculate(String source)
        {
            return 0;
        }

        private List<IIntegerBinaryOperator> _operators;
    }

    /// <summary>
    /// 二目运算符
    /// </summary>
    public interface IIntegerBinaryOperator
    {
        Boolean HitOperator(String input);
        void Calculate(Int32 left, Int32 right);
    }
}
