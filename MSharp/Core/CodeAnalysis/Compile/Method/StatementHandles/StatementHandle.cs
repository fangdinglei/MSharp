using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        public abstract List<Type> Types { get; }

        public abstract void Handle(StatementHandleParameters parameters);






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
                parameters.Block.VariableTable.Add(typeInfo, variable.Identifier.ToString());
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
                    var methodType = p.SemanticModel.GetSymbolInfo(memberCall.Name).Symbol as IMethodSymbol;
                    Debug.Assert(methodType != null);
                    if (!p.Context.Methods.TryGetValue(methodType, out var method))
                        throw new Exception("未知调用 拒绝访问");
                    p.Block.Calls.Add(method);
                    if (method.CallMode == MethodCallMode.Inline)
                    {
                        // 内联可能引发循环依赖
                        // 取消延迟编译并立即编译方法体
                        p.Context.WaitFurtherAnalyzing.Remove(methodType);
                        p.Context.MethodAnalyzer.AnalyzeMethodBody(
                            p.Context, methodType, method, true
                        );
                        Debug.Assert(method.Block != null);
                        p.Block.Merge(method.Block,"TODO 变量映射关系");
                    }
                    else if (method.CallMode == MethodCallMode.Stacked)
                    {
                        // 基于栈的调用 延迟编译
                        p.Context.WaitFurtherAnalyzing.Add(methodType, method);
                    }
                    else if (method.CallMode == MethodCallMode.UnsafeStacked)
                    {// p.Block.Merge(method.Block, method.CallMode);
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
            //    method.VariableTable.Add(typeInfo, variable.Identifier.ToString());
            //}
        }
    }

}
