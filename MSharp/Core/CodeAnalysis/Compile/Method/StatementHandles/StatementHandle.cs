using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Game;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.StatementHandles
{
    internal class StatementHandleParameters
    {
        public readonly CompileContext Context;
        public readonly MethodBodyAnalyzer MethodBodyAnalyzer;
        public readonly SemanticModel SemanticModel;
        public readonly LBlock Block;
        public readonly StatementSyntax Syntax;

        public StatementHandleParameters(CompileContext context, MethodBodyAnalyzer methodBodyAnalyzer
            , SemanticModel semanticModel, LBlock block, StatementSyntax syntax)
        {
            Context = context;
            MethodBodyAnalyzer = methodBodyAnalyzer;
            SemanticModel = semanticModel;
            Block = block;
            Syntax = syntax;
        }
    }
    internal abstract class StatementHandle
    {
        Dictionary<string, MindustryOperatorKind> BinaryOperatorMap = new()
        {
            { "+", MindustryOperatorKind.add},
            { "-", MindustryOperatorKind.sub},
        };

        Dictionary<string, MindustryOperatorKind> UnaryOperatorMap = new()
        {
            { "-", MindustryOperatorKind.sub},
        };

        public abstract List<Type> Types { get; }

        public abstract void Handle(StatementHandleParameters parameters);

        public LVariableOrValue ParseAsVariableOrValue(ExpressionSyntax expression, LMethod method, SemanticModel semanticModel)
        {
            if (expression is LiteralExpressionSyntax les)
                return new LVariableOrValue(les.Token.Value);

            if (expression is BinaryExpressionSyntax bes)
            {
                var typeInfo = semanticModel.GetTypeInfo(expression);
                Debug.Assert(typeInfo.Type != null);
                var variable = new LVariableOrValue(method.VariableTable.Add(typeInfo.Type));
                var kind = BinaryOperatorMap[bes.OperatorToken.Text];
                var left = ParseAsVariableOrValue(bes.Left, method, semanticModel);
                var right = ParseAsVariableOrValue(bes.Right, method, semanticModel);
                method.Emit(new Code_Operation(kind, variable, left, right));
                return new LVariableOrValue(variable);
            }
            // TODO like -1+1
            throw new NotImplementedException();
        }

        public LVariableOrValue ParseAsValue(LParameter argDefine)
        {
            return new LVariableOrValue(argDefine.DefaultValue);
        }

    }





    internal class LocalVariableStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() { typeof(LocalDeclarationStatementSyntax) };

        public override void Handle(StatementHandleParameters parameters)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)parameters.Syntax;
            var type = localDeclaration.Declaration.Type;
            var typeInfo = parameters.SemanticModel.GetTypeInfo(type);
            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                parameters.Block.Method.VariableTable.Add(typeInfo.Type, variable.Identifier.ToString());
            }
        }
    }

    internal class MethodCallStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(InvocationExpressionSyntax) ,
        };
        public override void Handle(StatementHandleParameters p)
        {
            ExpressionStatementSyntax ess = (ExpressionStatementSyntax)p.Syntax;
            InvocationExpressionSyntax ies = (InvocationExpressionSyntax)ess.Expression;

            MemberAccessExpressionSyntax? memberCall = ies.Expression as MemberAccessExpressionSyntax;
            IdentifierNameSyntax? selfCall = ies.Expression as IdentifierNameSyntax;

            if (memberCall != null)
            {
                // code like a.B();
                // objectType like a.GetType
                var objectType = p.SemanticModel.GetTypeInfo(memberCall.Expression);
                var gameObjectCall = p.Context.TypeUtility.IsSonOf(objectType.Type, typeof(GameObject));
                if (gameObjectCall)
                {
                    // Game API 游戏API调用，直接翻译
                }
                else
                {
                    // User Code 自定义函数，记录调用关系
                    var methodSymbol = p.SemanticModel.GetSymbolInfo(memberCall.Name).Symbol as IMethodSymbol;
                    Debug.Assert(methodSymbol != null);
                    if (!p.Context.Methods.TryGetValue(methodSymbol, out var methodCalled))
                        throw new Exception("未知调用 拒绝访问");
                    p.Block.Calls.Add(methodCalled);
                    if (methodCalled.CallMode == MethodCallMode.Inline)
                    {
                        // 内联可能引发循环依赖
                        // 取消延迟编译并立即编译方法体
                        p.Context.WaitFurtherAnalyzing.Remove(methodSymbol);
                        p.Context.MethodAnalyzer.AnalyzeMethodBody(
                            p.Context, methodSymbol, methodCalled, true
                        );


                        Debug.Assert(methodCalled.Block != null && methodCalled.Parameters != null);
                        Debug.Assert(ies.ArgumentList.Arguments.Count <= methodCalled.Parameters.Count);
                        Dictionary<LVariable, LVariableOrValue> dict = new();

                        // 解析实参后合并另一个函数的语句到当前语句
                        for (int i = 0; i < methodCalled.Parameters.Count; i++)
                        {
                            var argDefine = methodCalled.Parameters[i];
                            LVariableOrValue? variableOrValue;

                            Debug.Assert(argDefine.Used.HasValue, $"parameter used or not should be analyzed when call {nameof(p.Context.MethodAnalyzer.AnalyzeMethodBody)}");
                            if (!argDefine.Used.Value)
                                continue;

                            if (ies.ArgumentList.Arguments.Count > i)
                            {
                                // 正常解析
                                var arg = ies.ArgumentList.Arguments[i];
                                variableOrValue = ParseAsVariableOrValue(arg.Expression, p.Block.Method, p.SemanticModel);
                            }
                            else
                            {
                                // 后面使用默认参数
                                variableOrValue = ParseAsValue(argDefine);
                            }
                            dict.Add(argDefine.Variable, variableOrValue);
                        }
                        p.Block.Method.Merge(methodCalled, dict);

                    }
                    else if (methodCalled.CallMode == MethodCallMode.Stacked)
                    {
                        // 基于栈的调用 延迟编译
                        p.Context.WaitFurtherAnalyzing[methodSymbol] = methodCalled;
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
            // TODO
            //var localDeclaration = (LocalDeclarationStatementSyntax)syntax;
            //var type = localDeclaration.Declaration.Type;
            //var typeInfo = semanticModel.GetTypeInfo(type);
            //foreach (var variable in localDeclaration.Declaration.Variables)
            //{
            //    methodCalled.VariableTable.Add(typeInfo, variable.Identifier.ToString());
            //}
        }
    }

}
