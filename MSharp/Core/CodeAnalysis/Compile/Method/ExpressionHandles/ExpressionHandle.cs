using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Game;
using MSharp.Core.Shared;
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
        public record struct Parameter(
            ExpressionSyntax Syntax,
            CompileContext Context,
            SemanticModel SemanticMode,
            LBlock Block,
            LMethod Method,
            LVariableOrValue? Right=null
        )
        {
            public Parameter WithExpression(ExpressionSyntax expression) {
                return new Parameter(expression,Context,SemanticMode,Block,Method);
            }

            public Parameter WithRight(LVariableOrValue right) {
                Right = right;
                return new Parameter(Syntax, Context, SemanticMode, Block, Method,right);
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
        static public LVariableOrValue Assign(Parameter p)
        {
            Debug.Assert(p.Right!=null);
            var type = p.Syntax.GetType();
            while (type != null && type != typeof(object))
            {
                if (_handles.TryGetValue(type, out var handle))
                    return handle.DoAssign(p);
                type = type.BaseType;
            }
            throw new Exception("TODO not support   " + p.Syntax.ToString());
        }


        public abstract Type Syntax { get; }
        public abstract LVariableOrValue DoGetRight(Parameter p);
        public virtual LVariableOrValue DoAssign(Parameter p)
        {
            throw new Exception(GetType().FullName + "cannot be writer");
        }
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
             { "==", MindustryOperatorKind.equal},
             { "<", MindustryOperatorKind.LT},
        };
        public override Type Syntax => typeof(BinaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            Debug.Assert(syntax is BinaryExpressionSyntax);
            var bes = (BinaryExpressionSyntax)syntax;
            var typeInfo = semanticModel.GetTypeInfo(bes);
            Debug.Assert(typeInfo.Type != null);
            var variable = new LVariableOrValue(method.VariableTable.Add(typeInfo.Type));
            var kind = BinaryOperatorMap[bes.OperatorToken.Text];
            var left = GetRight(p.WithExpression(bes.Left));
            var right = GetRight(p.WithExpression(bes.Right));
            p.Block.Emit(new Code_Operation(kind, variable, left, right));
            return variable;
        }
    }

    /// <summary>
    /// 字面量
    /// </summary>
    internal class LiteralExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(LiteralExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
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

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            Debug.Assert(syntax is IdentifierNameSyntax);
            var lns = (IdentifierNameSyntax)syntax;
            LVariable? res;
            //本地变量
            method.VariableTable.TryGet(semanticModel.GetSymbolInfo(lns).Symbol!,out res);
            // 类成员
            if (res == null)
            {// todo 临时用一下
                res = new LVariable(lns.Identifier.Value as string);
            }
            // TODO 
            return new LVariableOrValue(res);
        }

        public override LVariableOrValue DoAssign(Parameter p)
        {
            var left = DoGetRight(p).Variable!;
            p.Method.Block!.Emit(new Code_Assign(left, p.Right!));
            return new LVariableOrValue(left);
        }
    }
    /// <summary>
    /// -1  -1.4 etc (negative numbers is not literal in C#)
    /// <br/>单目运算，如负数（负数应该也是字面量，但是C#给的是单目运算+字面量）
    /// </summary>
    internal class PrefixUnaryExpressionHandle : ExpressionHandle
    {
        Dictionary<string, MindustryOperatorKind> UnaryOperatorMap = new()
        {
            { "-", MindustryOperatorKind.sub},
        };
        public override Type Syntax => typeof(PrefixUnaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
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
            else if (pues.OperatorToken.Text == "++")
            {
                var w = new LVariableOrValue(Assign(p.WithExpression(pues.Operand)));
                if (w.Variable!.Kind == LVariable.VariableType.MemberVisit)
                {
                    throw new Exception("++ -- not allowed to change game data");
                }
                p.Block.Emit(new Code_Operation(MindustryOperatorKind.add, w, w, new LVariableOrValue(1)));
                return w;
            }
            else if (pues.OperatorToken.Text == "--")
            {
                var w = new LVariableOrValue(Assign(p.WithExpression(pues.Operand)));
                if (w.Variable!.Kind == LVariable.VariableType.MemberVisit)
                {
                    throw new Exception("++ -- not allowed to change game data");
                }
                p.Block.Emit(new Code_Operation(MindustryOperatorKind.sub, w, w, new LVariableOrValue(1)));
                return w;
            }
            throw new Exception();
        }
    }
    internal class PostExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(PostfixUnaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is PostfixUnaryExpressionSyntax);
            var pues = (PostfixUnaryExpressionSyntax)syntax;

            var right = GetRight(p.WithExpression(pues.Operand));
      
            if (pues.OperatorToken.Text == "++")
            {
                // Compatible with game data reading and writing
                var res = new LVariableOrValue(p.Method.VariableTable.Add(right.Variable!.Type!));
                p.Block.PostCodes.Add(new Code_Operation(MindustryOperatorKind.add, res, right, new LVariableOrValue(1)));
                var t = p.Block.Codes;
                p.Block.Codes = new List<BaseCode>();
                Assign(p.WithExpression(pues.Operand).WithRight(res));
                p.Block.PostCodes.InsertRange(0,p.Block.Codes);
                p.Block.Codes = t;
                return res;
            }
            else if (pues.OperatorToken.Text == "--")
            {
                 var res = new LVariableOrValue(p.Method.VariableTable.Add(right.Variable!.Type!));
                p.Block.PostCodes.Add(new Code_Operation(MindustryOperatorKind.sub, res, right, new LVariableOrValue(1)));
                var t = p.Block.Codes;
                p.Block.Codes = new List<BaseCode>();
                Assign(p.WithExpression(pues.Operand).WithRight(res));
                p.Block.PostCodes.InsertRange(0, p.Block.Codes);
                p.Block.Codes = t;
                return res;
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

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is ParenthesizedExpressionSyntax);
            return GetRight(p.WithExpression(((ParenthesizedExpressionSyntax)syntax).Expression));
        }
    }

    internal class InvocationExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(InvocationExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
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
                    method.Calls.Add(methodCalled);
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
                                variableOrValue = ExpressionHandle.GetRight(p.WithExpression(arg.Expression));
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

            return GetRight(p.WithExpression(((InvocationExpressionSyntax)syntax).Expression));
        }
    }

    internal class AssignmentExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(AssignmentExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is AssignmentExpressionSyntax);
            AssignmentExpressionSyntax aes = (AssignmentExpressionSyntax)syntax;
            var right= GetRight(p.WithExpression(aes.Right));
          return  Assign(p.WithExpression(aes.Left).WithRight(right));
        }
    }

    internal class MemberAccessExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(MemberAccessExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is MemberAccessExpressionSyntax);
            MemberAccessExpressionSyntax maes = (MemberAccessExpressionSyntax)syntax;

            var left = GetRight(p.WithExpression(maes.Expression));
            var right = maes.Name.Identifier.Value;
            Debug.Assert(left.IsVariable, "member access:left value must be variable");
            if (!CheckGameObjectData(maes.Expression, maes.Name, context, semanticModel))
                throw new Exception("oop not allowed");
            LVariableOrValue sensorVar = new LVariableOrValue(method.VariableTable.Add(semanticModel.GetTypeInfo(maes).Type!));
            p.Block.Emit(new Code_Sensor(sensorVar, left, "@" + (string)right!));
            return sensorVar;
        }
        public override LVariableOrValue DoAssign(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is MemberAccessExpressionSyntax);
            MemberAccessExpressionSyntax maes = (MemberAccessExpressionSyntax)syntax;

            var left = GetRight(p.WithExpression(maes.Expression));
            var right = maes.Name.Identifier.Value! as string;
            Debug.Assert(left.IsVariable, "member access:left value must be variable");
            //TODO check what can be write
            if (!CheckGameObjectData( maes.Expression, maes.Name, context, semanticModel))
                throw new Exception("oop not allowed");

            if (p.Context.TypeUtility.IsSonOf(,typeof(Building)))
            {
                //todo check building control or unit control
                p.Block.Emit(new Code_Control(left, right));
            }
          

            return p.Right!;


        }
        private bool CheckGameObjectData(ExpressionSyntax left, SimpleNameSyntax right, CompileContext context, SemanticModel semanticModel)
        {
            if (left is not IdentifierNameSyntax)
                return false;
            var type = semanticModel.GetTypeInfo(left).Type!;
            if (context.TypeUtility.IsSonOf(type, typeof(GameObject))
                && context.TypeUtility.HasAttribute(semanticModel.GetSymbolInfo(right).Symbol!, typeof(GameObjectDataAttribute)))
            {
                return true;
            }
            return false;
        }

    }

    internal class TupleExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(TupleExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is TupleExpressionSyntax);
            TupleExpressionSyntax tes = (TupleExpressionSyntax)syntax;

            var res = new List<object>();
            foreach (var item in tes.Arguments)
                res.Add(GetRight(p.WithExpression(item.Expression)));
            return new LVariableOrValue(res);
        }
    }

    internal class CastExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(CastExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is CastExpressionSyntax);
            CastExpressionSyntax ces = (CastExpressionSyntax)syntax;
            return GetRight(p.WithExpression(ces.Expression));
        }

    }

}
