using System.Linq.Expressions;

namespace SimpleExpressions
{
    internal interface IOptimizer
    {
        /// <summary>
        /// Tryes to build optimized expression
        /// </summary>
        /// <typeparam name="T">Expression type to ease lambda compilation</typeparam>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        bool TryBuild<T>(Expression<T> sourceExp, out Expression<T> optimizedExp);
    }
}
