using SimpleExpressions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Test
{
    class Program
    {
        private static int _counter;

        static void Main(string[] args)
        {
            // Unoptimized source expression with repeating calls of expensive function: F(x), F(y), F(2*y)
            Expression<Func<int, int, int>> myExp =
                (int x, int y) => F(x) > F(y) ? F(x) : (F(x) < F(2 * y) ? F(2 * y) : F(y));

            Func<int, int, int> compiledExp = myExp.Compile();

            Console.WriteLine("Using unoptimized expression");
            Console.WriteLine($"  Result:  { compiledExp(2, 8) }");
            Console.WriteLine($"  Expensive method called:  { _counter } times");
            Console.WriteLine();

            _counter = 0;

            // Using SameMethodsCalls instance
            IOptimizer optimizer = new SameMethodsCalls();

            if (optimizer.TryBuild(myExp, out Expression<Func<int, int, int>> optimizedExp))
            {
                // Uncomment to check new expression DebugView property console output
                //Console.Write(GetDebugView(optimizedExp));

                compiledExp = optimizedExp.Compile();

                Console.WriteLine("Using optimized expression");
                Console.WriteLine($"  Result:  { compiledExp(2, 8) }");
                Console.WriteLine($"  Expensive method called:  { _counter } times");
                Console.WriteLine();
                Console.Write("Press Enter...");
            }

            Console.ReadLine();
        }

        // Any expensive method (just as example)
        private static int F(int p)
        {
            // Any long or expensive work here
            // ...
            _counter++;
            return p;
        }

        /// <summary>
        /// Gets string presentation of DebugView property
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        private static string GetDebugView(Expression exp)
        {
            if (exp == null)
                return null;

            var propertyInfo = typeof(Expression).GetProperty("DebugView",
                BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo.GetValue(exp) as string;
        }
    }
}
