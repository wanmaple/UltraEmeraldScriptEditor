using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompileSupport.Utils
{
    /// <summary>
    /// 不确定的有穷自动机(Non-Determined Finite Automata)。
    /// </summary>
    /// <typeparam name="S"></typeparam>
    /// <typeparam name="I"></typeparam>
    /// <typeparam name="R"></typeparam>
    public interface INdfa<S, I, R>
    {
        /// <summary>
        /// 所有状态的集合
        /// </summary>
        ISet<S> AllStates { get; }
        /// <summary>
        /// 可被接受的状态集合
        /// </summary>
        ISet<S> AcceptableStates { get; }
        /// <summary>
        /// 之前的状态
        /// </summary>
        ISet<S> PreviousStates { get; }
        /// <summary>
        /// 当前的状态
        /// </summary>
        ISet<S> CurrentStates { get; }
        /// <summary>
        /// 起始状态
        /// </summary>
        S StartState { get; }
        /// <summary>
        /// 错误状态
        /// </summary>
        S ErrorState { get; }
        /// <summary>
        /// 转换表
        /// </summary>
        IDictionary<IPairKey<S, I>, ISet<S>> TransitionTable { get; }
        /// <summary>
        /// 转换函数
        /// </summary>
        IDictionary<IPairKey<S, S>, Func<S, S, R>> TransitionFunctions { get; }
        /// <summary>
        /// 错误处理函数
        /// </summary>
        Action<INdfa<S, I, R>, I> ErrorHandler { get; }
        /// <summary>
        /// 是否处于错误状态
        /// </summary>
        Boolean InErrorState { get; }
        /// <summary>
        /// 接受输入
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        R Receive(I input);
    }
}
