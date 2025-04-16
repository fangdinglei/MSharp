using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.Method.StatementHandles;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Exceptions;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using static MSharp.Core.Compile.TypeUtility;

namespace MSharp.Core.Compile.Method
{
    static internal class MethodAnalyzer
    {

        /// <summary>
        /// analyze method declarations only(method first analyze)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="methods"></param>
        /// <param name="semanticModel"></param>
        /// <param name="lClass"></param>
        static public void AnalyzeDeclaration(CompileContext context, IMethodSymbol methodSymbol, SemanticModel semanticModel, LClass lClass)
        {
            // TODO 检查参数是否是 out 这样的
            if (methodSymbol.DeclaringSyntaxReferences.Length > 1)
                throw new SyntaxNotSupported("partial not supported:" + methodSymbol.ToString());

            if (methodSymbol.DeclaringSyntaxReferences.Length == 0)
            {
                Console.WriteLine("method ignored:" + methodSymbol);
                return;
            }

            var method = lClass.CreateMethod(methodSymbol);
            method.Parameters = AnalyzeMethodParameters(context, semanticModel, methodSymbol, method);

            // 分析调用类型
            AnalyzeCustomerCallAttribute(method, methodSymbol);
        }

        /// <summary>
        /// analyze method body(method second analyze)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="methodSymbol"></param>
        /// <param name="method"></param>
        /// <param name="checkCircularDependency"></param>
        /// <exception cref="Exception"></exception>
        static public void AnalyzeBody(CompileContext context, IMethodSymbol methodSymbol, LMethod method, bool checkCircularDependency, bool hasEndCode)
        {
            if (checkCircularDependency && context.Analyzing.ContainsKey(methodSymbol))
                throw new CompileError("circular dependency:" + methodSymbol.ToString());

            if (method.Block != null)
                return;
            context.Analyzing.Add(methodSymbol, method);
            try
            {
                MethodDeclarationSyntax methodSyntax = (MethodDeclarationSyntax)
                    methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var statements = methodSyntax!.Body!.Statements;
                var semanticModel = context.SemanticModels[methodSyntax.SyntaxTree];

                var block = new LBlock(method);
                method.Block = block;
                foreach (var statement in statements.ToList())
                {
                    StatementHandle.Handle(statement, context, semanticModel, block);
                }
                if (hasEndCode)
                    method.Block.Emit(new Code_End());
            }
            finally
            {
                context.Analyzing.Remove(methodSymbol);
            }
        }

        /// <summary>
        /// analyze method parameters
        /// </summary>
        /// <param name="context"></param>
        /// <param name="semanticModel"></param>
        /// <param name="methodSymbol"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        static private List<LParameter> AnalyzeMethodParameters(CompileContext context, SemanticModel semanticModel, IMethodSymbol methodSymbol, LMethod method)
        {
            List<LParameter> res = new List<LParameter>();
            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                var v = method.VariableTable.AddLocalVariable(parameterSymbol.Type, parameterSymbol, parameterSymbol.Name);
                LParameter parameter = new LParameter(v, parameterSymbol.HasExplicitDefaultValue ? parameterSymbol.ExplicitDefaultValue : null);
                res.Add(parameter);
            }
            return res;
        }

        /// <summary>
        /// determine if it has <see cref="CustomerCallAttribute"/>
        /// </summary>
        static private void AnalyzeCustomerCallAttribute(LMethod method, IMethodSymbol symbol)
        {
            var gameCallAttribute = symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass!) == typeof(CustomerCallAttribute).FullName).FirstOrDefault();
            if (gameCallAttribute != null)
            {
                method.CallMode = (MethodCallMode)(int)gameCallAttribute.ConstructorArguments[0].Value!;
            }
            else
            {
                method.CallMode = MethodCallMode.Default;
            }
        }

    }
}
