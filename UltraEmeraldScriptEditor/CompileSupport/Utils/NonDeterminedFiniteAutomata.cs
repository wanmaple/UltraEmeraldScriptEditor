using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    public class NonDeterminedFiniteAutomata<S, I, R> : INdfa<S, I, R>
    {
        public NonDeterminedFiniteAutomata()
        {
            _allStates = new HashSet<S>();
            _acceptableStates = new HashSet<S>();
            _prevStates = new SortedSet<S>();
            _curStates = new SortedSet<S>();
            _tranTable = new Dictionary<IPairKey<S, I>, ISet<S>>();
            _tranFuncs = new Dictionary<IPairKey<S, S>, Func<S, S, R>>();
            _inErrState = false;
        }

        public void Restart()
        {
            _prevStates.Clear();
            _curStates.Clear();
            _curStates.Add(_startState);
            _inErrState = false;
        }

        #region INdfa<S, I, R>
        public ISet<S> AllStates => _allStates;

        public ISet<S> AcceptableStates => _acceptableStates;

        public ISet<S> PreviousStates => _prevStates;

        public ISet<S> CurrentStates => _curStates;

        public S StartState
        {
            get => _startState;
            set => _startState = value;
        }

        public S ErrorState
        {
            get => _errState;
            set => _errState = value;
        }

        public IDictionary<IPairKey<S, I>, ISet<S>> TransitionTable => _tranTable;

        public IDictionary<IPairKey<S, S>, Func<S, S, R>> TransitionFunctions => _tranFuncs;

        public Action<INdfa<S, I, R>, I> ErrorHandler
        {
            get => _errHandler;
            set => _errHandler = value;
        }

        public bool InErrorState => _inErrState;

        public virtual R Receive(I input)
        {
            throw new NotImplementedException();
        }
        #endregion

        protected HashSet<S> _allStates;
        protected HashSet<S> _acceptableStates;
        protected SortedSet<S> _prevStates;
        protected SortedSet<S> _curStates;
        protected S _startState;
        protected S _errState;
        protected Dictionary<IPairKey<S, I>, ISet<S>> _tranTable;
        protected Dictionary<IPairKey<S, S>, Func<S, S, R>> _tranFuncs;
        protected Action<INdfa<S, I, R>, I> _errHandler;
        protected Boolean _inErrState;
    }
}
