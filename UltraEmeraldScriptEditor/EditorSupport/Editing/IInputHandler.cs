using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace EditorSupport.Editing
{
    public interface IInputHandler
    {
        EditView Owner { get; }
        void Attach();
        void Detach();
    }
}
