using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.Method.StatementHandles;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// 表达式处理基类
    /// </summary>
    internal abstract class ExpressionHandle
    {
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

        static public LVariableOrValue GetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            var type = syntax.GetType();
            while (type != null && type != typeof(object))
            {
                if (_handles.TryGetValue(type, out var handle))
                    return handle.DoGetValue(syntax, semanticModel, method);
                type = type.BaseType;
            }
            throw new Exception("TODO not support");
        }

        public abstract Type Syntax { get; }
        public abstract LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method);
    }

    /// <summary>
    /// + - *, etc 二元运算
    /// </summary>
    internal class BinaryExpressionHandle : ExpressionHandle
    {
        Dictionary<string, MindustryOperatorKind> BinaryOperatorMap = new()
        {
            { "+", MindustryOperatorKind.add},
            { "-", MindustryOperatorKind.sub},
        };
        public override Type Syntax => typeof(BinaryExpressionSyntax);
        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is BinaryExpressionSyntax);
            var bes = (BinaryExpressionSyntax)syntax;
            var typeInfo = semanticModel.GetTypeInfo(bes);
            Debug.Assert(typeInfo.Type != null);
            var variable = new LVariableOrValue(method.VariableTable.Add(typeInfo.Type));
            var kind = BinaryOperatorMap[bes.OperatorToken.Text];
            var left = GetValue(bes.Left, semanticModel, method);
            var right = GetValue(bes.Right, semanticModel, method);
            method.Emit(new Code_Operation(kind, variable, left, right));
            return new LVariableOrValue(variable);
        }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    internal class LiteralExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(LiteralExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is LiteralExpressionSyntax);
            var les = (LiteralExpressionSyntax)syntax;
            return new LVariableOrValue(les.Token.Value);
        }
    }
    /// <summary>
    /// Variable 变量取值
    /// </summary>
    internal class IdentifierNameHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(IdentifierNameSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is IdentifierNameSyntax);
            var lns = (IdentifierNameSyntax)syntax;
            //TODO 其他情况 这里这考虑本地变量
#pragma warning disable CS8604
            var v = method.VariableTable.Get(semanticModel.GetSymbolInfo(lns).Symbol);
#pragma warning restore CS8604
            return new LVariableOrValue(v);
        }
    }
    /// <summary>
    /// -1  -1.4 etc (negative numbers is not literal in C#)
    /// <br/>单目运算，如负数（负数应该也是字面量，但是C#给的是单目运算+字面量）
    /// </summary>
    internal class UnaryExpressionHandle : ExpressionHandle
    {
        Dictionary<string, MindustryOperatorKind> UnaryOperatorMap = new()
        {
            { "-", MindustryOperatorKind.sub},
        };
        public override Type Syntax => typeof(PrefixUnaryExpressionSyntax);
        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is PrefixUnaryExpressionSyntax);
            var pues = (PrefixUnaryExpressionSyntax)syntax;
            if (pues.OperatorToken.Text == "-")
            {
                if (pues.Operand is LiteralExpressionSyntax les)
                {
                    var v= les.Token.Value;
                    // INumber may help
                    if (v is int a)
                        return new LVariableOrValue(-a);
                    else if (v is uint b)
                        return new LVariableOrValue(-b);
                    else if (v is long c)
                        return new LVariableOrValue(-c);
                    else if (v is ulong d)
                        throw new Exception(" - is not allowed to operate ulong");
                    else if (v is short e)
                        return new LVariableOrValue(-e);
                    else if (v is ushort f)
                        return new LVariableOrValue(-f);
                    else if (v is byte g)
                        return new LVariableOrValue(-g);
                    else if (v is char h)
                        return new LVariableOrValue(-h);
                    else if (v is double i)
                        return new LVariableOrValue(-i);
                    else if (v is float j)
                        return new LVariableOrValue(-j);
                    else
                        throw new Exception(" - is not allowed to operate " +  v?.GetType().FullName??"null");
                }
            }
            else if (pues.OperatorToken.Text == "+")
            {
                if (pues.Operand is LiteralExpressionSyntax les) {
                    return new LVariableOrValue(les.Token.Value);
                }
            }
            throw new Exception();
        }
    }

    /// <summary>
    /// () Parenthesized Expression
    /// <br/>括号语句，直接让处理子项
    /// </summary>
    internal class ParenthesizedExpressionHandle:ExpressionHandle {
        public override Type Syntax => typeof(ParenthesizedExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is ParenthesizedExpressionSyntax);
            return GetValue(((ParenthesizedExpressionSyntax)syntax).Expression,semanticModel,method);
        }
    }

}
