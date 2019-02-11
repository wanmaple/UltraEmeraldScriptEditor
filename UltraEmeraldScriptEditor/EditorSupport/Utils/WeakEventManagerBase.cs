using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace EditorSupport.Utils
{
    public abstract class WeakEventManagerBase<TPublisher, TManager> : WeakEventManager
        where TPublisher : class
        where TManager : WeakEventManagerBase<TPublisher, TManager>, new()
    {
        #region Abstraction
        protected abstract void StartListening(TPublisher publisher);
        protected abstract void StopListening(TPublisher publisher);
        #endregion

        #region Overrides
        protected override void StartListening(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            StartListening(source as TPublisher);
        }

        protected override void StopListening(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            StopListening(source as TPublisher);
        }
        #endregion

        public static void AddListener(TPublisher publisher, IWeakEventListener listener)
        {
            CurrentManager.ProtectedAddListener(publisher, listener);
        }

        public static void RemoveListener(TPublisher publisher, IWeakEventListener listener)
        {
            CurrentManager.ProtectedRemoveListener(publisher, listener);
        }

        protected static TManager CurrentManager
        {
            get
            {
                Type managerType = typeof(TManager);
                TManager manager = GetCurrentManager(managerType) as TManager;
                if (manager == null)
                {
                    manager = new TManager();
                    SetCurrentManager(managerType, manager);
                }
                return manager;
            }
        }
    }
}
