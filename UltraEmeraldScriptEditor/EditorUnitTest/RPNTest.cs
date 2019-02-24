using System;
using System.Diagnostics;
using CompileSupport.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EditorUnitTest
{
    [TestClass]
    public class RPNTest
    {
        [TestMethod]
        public void TestRPNIntegerCalculator()
        {
            var calc = new RPNIntegerCalculator();

            Debug.WriteLine("Result: {0}", calc.Calculate("0xce"));
            Debug.WriteLine("Result: {0}", calc.Calculate("1+2*3-4"));
            Debug.WriteLine("Result: {0}", calc.Calculate("(1+2*3+7)/2"));
            Debug.WriteLine("Result: {0}", calc.Calculate("((1+2)*3-4*1)*(2+1*3)"));
            Debug.WriteLine("Result: {0}", calc.Calculate("0x2 << (1 + 2)"));
            Debug.WriteLine("Result: {0}", calc.Calculate("0xFF >> (1 + 2)"));
            Debug.WriteLine("Result: {0}", calc.Calculate("0x1 & 0x3"));
            Debug.WriteLine("Result: {0}", calc.Calculate("0x1 | 0x3"));
            Debug.WriteLine("Result: {0}", calc.Calculate("0x1 ^ 0x3"));
            Debug.WriteLine("Result: {0}", calc.Calculate("(0x3<<(0x1+0x2))*((0xff-0x3*(0xcf>>0x4))&(0xcc^(0xae/0x2-(0x1|0x2)+(0x5&0xf))))"));
            Debug.WriteLine((0x3 << (0x1 + 0x2)) * ((0xff - 0x3 * (0xcf >> 0x4)) & (0xcc ^ (0xae / 0x2 - (0x1 | 0x2) + (0x5 & 0xf)))));
        }
    }
}
