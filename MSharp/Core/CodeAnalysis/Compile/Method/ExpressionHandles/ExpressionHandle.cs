using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Game;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

        static public LVariableOrValue GetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            var type = syntax.GetType();
            while (type != null && type != typeof(object))
            {
                if (_handles.TryGetValue(type, out var handle))
                    return handle.DoGetValue(syntax, context, semanticModel, method);
                type = type.BaseType;
            }
            throw new Exception("TODO not support   " + syntax.ToString());
        }

        static public LVariableOrValue Assign(LVariable left, ExpressionSyntax exp, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            var l = new LVariableOrValue(left);
            method.Emit(new Code_Assign(l, GetValue(exp, context, semanticModel, method)));
            return l;
        }

        public abstract Type Syntax { get; }
        public abstract LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method);
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
        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is BinaryExpressionSyntax);
            var bes = (BinaryExpressionSyntax)syntax;
            var typeInfo = semanticModel.GetTypeInfo(bes);
            Debug.Assert(typeInfo.Type != null);
            var variable = new LVariableOrValue(method.VariableTable.Add(typeInfo.Type));
            var kind = BinaryOperatorMap[bes.OperatorToken.Text];
            var left = GetValue(bes.Left, context, semanticModel, method);
            var right = GetValue(bes.Right, context, semanticModel, method);
            method.Emit(new Code_Operation(kind, variable, left, right));
            return variable;
        }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    internal class LiteralExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(LiteralExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
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

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
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
        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is PrefixUnaryExpressionSyntax);
            var pues = (PrefixUnaryExpressionSyntax)syntax;
            if (pues.OperatorToken.Text == "-")
            {
                if (pues.Operand is LiteralExpressionSyntax les)
                {
                    var v = les.Token.Value;
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
                        throw new Exception(" - is not allowed to operate " + v?.GetType().FullName ?? "null");
                }
            }
            else if (pues.OperatorToken.Text == "+")
            {
                if (pues.Operand is LiteralExpressionSyntax les)
                {
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
    internal class ParenthesizedExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(ParenthesizedExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is ParenthesizedExpressionSyntax);
            return GetValue(((ParenthesizedExpressionSyntax)syntax).Expression, context, semanticModel, method);
        }
    }

    internal class InvocationExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(InvocationExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is InvocationExpressionSyntax);

            InvocationExpressionSyntax ies = (InvocationExpressionSyntax)syntax;

            MemberAccessExpressionSyntax? memberCall = ies.Expression as MemberAccessExpressionSyntax;
            IdentifierNameSyntax? selfCall = ies.Expression as IdentifierNameSyntax;

            if (memberCall != null)
            {
                // code like a.B();
                // objectType like a.GetType
                var objectType = semanticModel.GetTypeInfo(memberCall.Expression);
                var gameObjectCall = context.TypeUtility.IsSonOf(objectType.Type, typeof(GameObject));
                if (gameObjectCall)
                {
                    // Game API 游戏API调用，直接翻译
                }
                else
                {
                    // User Code 自定义函数，记录调用关系
                    var methodSymbol = semanticModel.GetSymbolInfo(memberCall.Name).Symbol as IMethodSymbol;
                    Debug.Assert(methodSymbol != null);
                    if (!context.Methods.TryGetValue(methodSymbol, out var methodCalled))
                        throw new Exception("未知调用 拒绝访问");
                    method.Block!.Calls.Add(methodCalled);
                    if (methodCalled.CallMode == MethodCallMode.Inline)
                    {
                        // 内联可能引发循环依赖
                        // 取消延迟编译并立即编译方法体
                        context.WaitFurtherAnalyzing.Remove(methodSymbol);
                        context.MethodAnalyzer.AnalyzeMethodBody(
                           context, methodSymbol, methodCalled, true
                        );


                        Debug.Assert(methodCalled.Block != null && methodCalled.Parameters != null);
                        Debug.Assert(ies.ArgumentList.Arguments.Count <= methodCalled.Parameters.Count);
                        Dictionary<LVariable, LVariableOrValue> dict = new();

                        // 解析实参后合并另一个函数的语句到当前语句
                        for (int i = 0; i < methodCalled.Parameters.Count; i++)
                        {
                            var argDefine = methodCalled.Parameters[i];
                            LVariableOrValue? variableOrValue;

                            Debug.Assert(argDefine.Used.HasValue, $"parameter used or not should be analyzed when call {nameof(context.MethodAnalyzer.AnalyzeMethodBody)}");
                            if (!argDefine.Used.Value)
                                continue;

                            if (ies.ArgumentList.Arguments.Count > i)
                            {
                                // 正常解析
                                var arg = ies.ArgumentList.Arguments[i];
                                variableOrValue = ExpressionHandle.GetValue(arg.Expression, context, semanticModel, method);
                            }
                            else
                            {
                                // 后面使用默认参数
                                variableOrValue = null;//s ParseAsValue(argDefine);
                            }
                            dict.Add(argDefine.Variable, variableOrValue);
                        }
                        method.Merge(methodCalled, dict);

                    }
                    else if (methodCalled.CallMode == MethodCallMode.Stacked)
                    {
                        // 基于栈的调用 延迟编译
                        context.WaitFurtherAnalyzing[methodSymbol] = methodCalled;
                    }
                    else if (methodCalled.CallMode == MethodCallMode.UnsafeStacked)
                    {// p.Method.Merge(methodCalled.Method, methodCalled.CallMode);
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
            else if (selfCall != null)
            {

            }
            else
            {
                throw new Exception();
            }

            return GetValue(((InvocationExpressionSyntax)syntax).Expression, context, semanticModel, method);
        }
    }

    internal class AssignmentExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(AssignmentExpressionSyntax);

        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is AssignmentExpressionSyntax);
            AssignmentExpressionSyntax aes = (AssignmentExpressionSyntax)syntax;

            var left = GetValue(aes.Left, context, semanticModel, method);
            Debug.Assert(left.Variable != null);
            return Assign(left.Variable, aes.Right, context, semanticModel, method);
        }
    }

    internal class MemberAccessExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(MemberAccessExpressionSyntax);
        public override LVariableOrValue DoGetValue(ExpressionSyntax syntax, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            Debug.Assert(syntax is MemberAccessExpressionSyntax);
            MemberAccessExpressionSyntax maes = (MemberAccessExpressionSyntax)syntax;
            LVariableOrValue? res = null;
            // left.right such as a.b
            if ((res = CheckSensor(maes, maes.Expression, maes.Name, context, semanticModel, method)) != null)
            {
                return res;
            }
            var left = GetValue(maes.Expression, context, semanticModel, method);
            return null;
        }

        private LVariableOrValue? CheckSensor(MemberAccessExpressionSyntax maes, ExpressionSyntax left, SimpleNameSyntax right, CompileContext context, SemanticModel semanticModel, LMethod method)
        {
            if (left is not IdentifierNameSyntax name)
                return null;
            var type = semanticModel.GetTypeInfo(left).Type!;
            if (context.TypeUtility.IsSonOf(type, typeof(GameObject))
                && context.TypeUtility.HasAttribute(semanticModel.GetSymbolInfo(right).Symbol!, typeof(GameSensorFieldAttribute)))
            {
                LVariableOrValue result = new LVariableOrValue(method.VariableTable.Add(semanticModel.GetTypeInfo(maes).Type!));
                var obj = new LVariableOrValue(method.VariableTable.Add(type, semanticModel.GetSymbolInfo(left).Symbol!, $"{name.Identifier.Value}"));
                // game var
                // a.@b    Identifier.Text:@b   Identifier.Value:b
                method.Emit(new Code_Sensor(result, obj, "@" + (string)right.Identifier.Value!));
                return result;
            }
            return null;
        }

    }
}
