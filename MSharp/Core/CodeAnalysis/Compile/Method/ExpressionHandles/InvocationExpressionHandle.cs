using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Game;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static MSharp.Core.CodeAnalysis.Compile.TypeUtility;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
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
                // code like a.B(xxx);
                // objectType like a.GetType
                var objectType = semanticModel.GetTypeInfo(memberCall.Expression);
                //var methodType = semanticModel.GetSymbolInfo(memberCall).Symbol as IMethodSymbol;
                semanticModel.GetDeclaredSymbol(memberCall);
                string? gameApiName = null;
                bool needTarget = false;
                int parameterCount = 0;
                int targetIndex = 0;
                var gameObjectCall = IsSonOf(objectType.Type, typeof(GameObject))
                    && GetGameApiName(semanticModel.GetSymbolInfo(memberCall).Symbol!, out gameApiName, out parameterCount, out needTarget, out targetIndex);
                if (gameObjectCall)
                {
                    // Parameters such as [ out int a ] can also be processed in this way, because in C #, their arguments can only be newly defined variables or previously defined variables
                    // out int a 这样的参数在这里也可以这样被处理，因为在C#中他们的实参只能是新定义的变量或者之前定义的变量
                    var argList = ies.ArgumentList.Arguments
                          .Select(arg => GetRight(p.WithExpression(arg.Expression)))
                          .ToList();
                    if (needTarget)
                    {
                        argList.Insert(targetIndex, GetRight(p.WithExpression(memberCall.Expression)));
                    }

                    p.Block.Emit(new Code_Command(gameApiName!, new LVariableOrValue(argList, parameterCount)));

                    return null;

                }
                else
                {
                    // User Code 自定义函数，记录调用关系
                    var methodSymbol = semanticModel.GetSymbolInfo(memberCall.Name).Symbol as IMethodSymbol;
                    Debug.Assert(methodSymbol != null);
                    if (!context.Methods.TryGetValue(methodSymbol, out var methodCalled))
                        throw new Exception("未知调用 拒绝访问");
                    method.RecordCallTo(methodCalled);
                    if (methodCalled.CallMode == MethodCallMode.Inline)
                    {
                        // 内联可能引发循环依赖
                        // 取消延迟编译并立即编译方法体
                        context.WaitFurtherAnalyzing.Remove(methodSymbol);
                        MethodAnalyzer.AnalyzeBody(
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

                            Debug.Assert(argDefine.Used.HasValue, $"parameter used or not should be analyzed when call {nameof(MethodAnalyzer.AnalyzeBody)}");
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
                                variableOrValue = null;//fullName ParseAsValue(argDefine);
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

        /// <summary>
        /// 获取 api 的信息
        /// <br/> <see cref="GameApiAttribute"/>
        /// </summary>
        /// <param name="method"></param>
        private bool GetGameApiName(ISymbol symbol, out string? apiName, out int parameterCount, out bool needTarget, out int targetIndex)
        {
            apiName = null; parameterCount = 0; needTarget = false; targetIndex = 0;
            var att = symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass!) == typeof(GameApiAttribute).FullName).FirstOrDefault();
            if (att == null)
                return false;
            apiName = (string)att.ConstructorArguments[0].Value!;
            parameterCount = (int)att.ConstructorArguments[1].Value!;
            needTarget = (bool)att.ConstructorArguments[2].Value!;
            targetIndex = (int)att.ConstructorArguments[3].Value!;
            return true;
        }

    }

}
