using System;
using System.Diagnostics;
using EditorSupport.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EditorUnitTest
{
    [TestClass]
    public class ValueSequenceTest
    {
        [TestMethod]
        public void TestCreation()
        {
            var seq = CreateSequence();
            Debug.WriteLine(seq.ToString());
        }

        [TestMethod]
        public void TestGetters()
        {
            var seq = CreateSequence();
            for (int i = 0; i < seq.Count; i++)
            {
                Debug.WriteLine("{0} -> {1}", i, seq[i]);
            }
        }

        [TestMethod]
        public void TestEnumeration()
        {
            var seq = CreateSequence();
            foreach (var val in seq)
            {
                Debug.Write(val.ToString() + "\t");
            }
        }

        [TestMethod]
        public void TestAdd()
        {
            var seq = CreateSequence();
            seq.Add(10.1);
            seq.Add(5.0);
            seq.AddRange(new Double[] { 5.0, 4.999999999999999, 5.0, 10.1, 10.1, 10.1, });
            Debug.WriteLine(seq.ToString());
        }

        [TestMethod]
        public void TestInsertion()
        {
            var seq = CreateSequence();
            seq.Insert(seq.Count, 10.1);
            seq.InsertRange(seq.Count, new Double[] { 10.1, 5.5, 5.5, 2.0, });
            seq.Insert(6, 5.0);
            seq.Insert(7, 6.0);
            seq.InsertRange(7, new Double[] { 4.0, 4.0, 6.0, 6.0, });
            seq.Insert(7, 5.5);
            seq.Insert(3, 5.0);
            seq.Insert(3, 6.5);
            seq.InsertRange(3, new Double[] { 5.0, 5.0, 6.5, 6.5, });
            seq.Insert(0, 5.0);
            seq.Insert(0, 1.0);
            seq.InsertRange(0, new Double[] { 1.0, 1.0, 1.0, 2.5, 2.5, 1.0, 1.0, });
            Debug.WriteLine(seq.ToString());
        }

        [TestMethod]
        public void TestRemoval()
        {
            var seq = CreateSequence();
            seq.RemoveAt(0);
            seq.RemoveRange(1, 2);
            seq.RemoveRange(2, 5);
            seq.RemoveRange(0, 9);
            Debug.WriteLine(seq.ToString());
        }

        private ValueSequence CreateSequence()
        {
            var values = new Double[] { 5.0, 5.0, 5.0, 5.0, 5.0, 5.000000001, 6.0, 6.0, 6.0, 6.0, 6.0, 1.0, 2.0, 2.0, 5.5, 5.5, 5.0, 5.0, 5.0, 10.1, 10.1, 10.1, };
            var seq = new ValueSequence(values);
            return seq;
        }
    }
}
