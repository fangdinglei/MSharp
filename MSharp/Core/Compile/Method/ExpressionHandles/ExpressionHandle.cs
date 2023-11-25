using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSharp.Core.Compile.Method.ExpressionHandles
{

    /// <summary>
    /// 表达式处理基类
    /// <br/> right value is for read,left value is for write
    /// </summary>
    internal abstract class ExpressionHandle
    {
        public record struct Parameter(
            ExpressionSyntax Syntax,
            CompileContext Context,
            SemanticModel SemanticMode,
            LBlock Block,
            LMethod Method,
            LVariableOrValue? Right = null
        )
        {
            public Parameter WithExpression(ExpressionSyntax expression)
            {
                return new Parameter(expression, Context, SemanticMode, Block, Method, Right);
            }

            public Parameter WithRight(LVariableOrValue right)
            {
                return new Parameter(Syntax, Context, SemanticMode, Block, Method, right);
            }
        }

        static public Dictionary<Type, ExpressionHandle> _handles = new();

        static ExpressionHandle()
        {
            typeof(ExpressionHandle).Assembly.GetTypes()
                .Where(it =>
                {
                    Type? ptr = it;
                    while (true)
                    {
                        if (ptr == null || ptr == typeof(object))
                            return false;
                        if (ptr.BaseType == typeof(ExpressionHandle))
                            return true;
                        ptr = ptr.BaseType;
                    }
                })
                .Select(it => (ExpressionHandle?)Activator.CreateInstance(it))
                .ToList().ForEach(it =>
                {
                    Debug.Assert(it != null);
                    _handles.Add(it.Syntax, it);
                });
        }

        static public LVariableOrValue GetRight(Parameter p)
        {
            var type = p.Syntax.GetType();
            while (type != null && type != typeof(object))
            {
                if (_handles.TryGetValue(type, out var handle))
                    return handle.DoGetRight(p);
                type = type.BaseType;
            }
            throw new Exception("TODO not support   " + p.Syntax.ToString());
        }
        static public LVariableOrValue GetRightAndMergePostCode(Parameter p)
        {
            var r = GetRight(p);
            p.Block.MergePostCodes();
            return r;
        }
        static public LVariableOrValue Assign(Parameter p)
        {
            Debug.Assert(p.Right != null);
            var type = p.Syntax.GetType();
            while (type != null && type != typeof(object))
            {
                if (_handles.TryGetValue(type, out var handle))
                    return handle.DoAssign(p);
                type = type.BaseType;
            }
            throw new Exception("TODO not support   " + p.Syntax.ToString());
        }
        static public LVariableOrValue Assign(LVariable left, LVariableOrValue right, LBlock block, bool postCode = false)
        {
            block.Emit(new Code_Assign(left, right), postCode);
            return right;
        }

        /// <summary>
        /// match expression of this type
        /// </summary>
        public abstract Type Syntax { get; }
        /// <summary>
        /// get value of Expression
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public abstract LVariableOrValue DoGetRight(Parameter p);
        /// <summary>
        /// assign right value to left Expression  
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual LVariableOrValue DoAssign(Parameter p)
        {
            throw new Exception(GetType().FullName + "cannot be writer");
        }
    }

}
