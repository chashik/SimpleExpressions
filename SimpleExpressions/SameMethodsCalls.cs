using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace SimpleExpressions
{
    /// <summary>
    /// Substitutes repeating calls with condition expression wich places variable 
    /// instead of method call and assign it during run-time only if neccessary
    /// </summary>
    public class SameMethodsCalls : IOptimizer
    {
        private readonly object _lockObj;
        private Dictionary<string, ParameterExpression> _substitutions;
        private Dictionary<string, ParameterExpression> _assigned;

        public SameMethodsCalls()
        {
            _lockObj = new object();

            _substitutions = new Dictionary<string, ParameterExpression>();
            _assigned = new Dictionary<string, ParameterExpression>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        public bool TryBuild<T>(Expression<T> sourceExp, out Expression<T> optimizedExp)
        {
            try
            {
                Monitor.Enter(_lockObj);
                optimizedExp = (Expression<T>)SubstituteMethodsCalls(sourceExp);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not optimize source expression!");
                Console.WriteLine(ex.Message);
                optimizedExp = sourceExp;
                return false;
            }
            finally
            {
                Release();
                Monitor.Exit(_lockObj);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Release()
        {
            _substitutions.Clear();
            _assigned.Clear();
        }

        /// <summary>
        /// Recursively visits each expression node and replaces methods calls
        /// </summary>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        private Expression SubstituteMethodsCalls(Expression sourceExp)
        {
            switch (sourceExp.NodeType)
            {
                case ExpressionType.Constant:
                    return sourceExp;

                case ExpressionType.Lambda:
                    var lambda = (LambdaExpression)sourceExp;
                    var replaced = SubstituteMethodsCalls(lambda.Body);
                    var declarations = _substitutions.Values.Concat(_assigned.Values).ToArray();
                    var block = Expression.Block(declarations, replaced);

                    return Expression.Lambda(block, lambda.Parameters);

                case ExpressionType.Parameter:
                    return sourceExp;

                case ExpressionType.Call:
                    return SubstituteMethodCall((MethodCallExpression)sourceExp);

                case ExpressionType.Conditional:
                    var conditional = (ConditionalExpression)sourceExp;
                    var test = SubstituteMethodsCalls(conditional.Test);
                    var ifTrue = SubstituteMethodsCalls(conditional.IfTrue);
                    var ifFalse = SubstituteMethodsCalls(conditional.IfFalse);

                    return Expression.Condition(test, ifTrue, ifFalse);

                case ExpressionType.Add:

                    var binary = (BinaryExpression)sourceExp;
                    var left = SubstituteMethodsCalls(binary.Left);
                    var right = SubstituteMethodsCalls(binary.Right);

                    return Expression.Add(left, right);

                case ExpressionType.LessThan:

                    binary = (BinaryExpression)sourceExp;
                    left = SubstituteMethodsCalls(binary.Left);
                    right = SubstituteMethodsCalls(binary.Right);

                    return Expression.LessThan(left, right);

                case ExpressionType.GreaterThan:

                    binary = (BinaryExpression)sourceExp;
                    left = SubstituteMethodsCalls(binary.Left);
                    right = SubstituteMethodsCalls(binary.Right);

                    return Expression.GreaterThan(left, right);

                case ExpressionType.Multiply:

                    binary = (BinaryExpression)sourceExp;
                    left = SubstituteMethodsCalls(binary.Left);
                    right = SubstituteMethodsCalls(binary.Right);

                    return Expression.Multiply(left, right);

                default:
                    Console.Error.WriteLine($"{sourceExp.NodeType} node is not yet implemented.");
                    return Expression.Constant("Undefined");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceExp"></param>
        /// <returns></returns>
        private ConditionalExpression SubstituteMethodCall(MethodCallExpression sourceExp)
        {
            // Using string presentation of method call expression as Identifier
            var methodCallId = sourceExp.ToString();

            ParameterExpression substitutionVariable, isAssignedVariable;

            if (_substitutions.ContainsKey(methodCallId))
            {
                substitutionVariable = _substitutions[methodCallId];
                isAssignedVariable = _assigned[methodCallId];
            }
            else
            {
                substitutionVariable = Expression.Variable(sourceExp.Method.ReturnType);
                isAssignedVariable = Expression.Variable(typeof(bool));
                _substitutions.Add(methodCallId, substitutionVariable);
                _assigned.Add(methodCallId, isAssignedVariable);
            }

            var assignmentBlock = Expression.Block(
                Expression.Assign(_assigned[methodCallId], Expression.Constant(true)),
                Expression.Assign(_substitutions[methodCallId], sourceExp));

            return Expression.Condition(isAssignedVariable, substitutionVariable, assignmentBlock);
        }
    }
}
