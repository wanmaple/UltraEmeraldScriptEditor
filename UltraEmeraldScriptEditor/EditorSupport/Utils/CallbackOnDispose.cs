using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace EditorSupport.Utils
{
    /// <summary>
    /// 在Dispose时触发的回调。
    /// </summary>
    public sealed class CallbackOnDispose : IDisposable
    {
        public CallbackOnDispose(Action action)
        {
            _action = action ?? throw new ArgumentNullException("action");
        }

        public void Dispose()
        {
            Action action = Interlocked.Exchange(ref _action, null);
            if (action != null)
            {
                action();
            }
        }

        private Action _action;
    }
}
