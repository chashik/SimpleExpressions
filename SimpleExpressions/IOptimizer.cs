using System.Linq.Expressions;

namespace SimpleExpressions
{
    public interface IOptimizer
    {
        /// <summary>
        /// Tryes to build optimized expression
        /// </summary>
        /// <typeparam name="T">Expression type to ease further lambda compilation</typeparam>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        bool TryBuild<T>(Expression<T> sourceExp, out Expression<T> optimizedExp);
    }
}
