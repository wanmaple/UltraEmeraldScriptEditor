using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using EditorSupport.Document;
using EditorSupport.Editing;

namespace EditorSupport.CodeCompletion
{
    public class DefaultCompletion : ICompletionData
    {
        public ImageSource Image => null;

        public string Text => "Test";

        public object Content => "Test";

        public object Description => throw new NotImplementedException();

        public int Priority => throw new NotImplementedException();

        public void PerformCompletion(EditView editview, ISegment segment)
        {
            throw new NotImplementedException();
        }
    }
}
