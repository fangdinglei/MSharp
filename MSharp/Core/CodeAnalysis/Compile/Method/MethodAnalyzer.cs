using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Logic;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSharp.Core.CodeAnalysis.Compile.Method
{
    internal class ClassAnalyzer : BaseAnalyzer
    {
        /// <summary>
        /// 是否是CPU逻辑
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns></returns>
        bool IsCPUClass(INamedTypeSymbol classSymbol)
        {
            if (classSymbol.IsAbstract)
                return false;
            // 继承自 class GameCPU
            INamedTypeSymbol? node = classSymbol.BaseType;
            while (node != null)
            {
                if (GetFullName(node) == typeof(GameCPU).FullName)
                {
                    return true;
                }
                node = node.BaseType;
            }
            return false;
        }

        /// <summary>
        /// 是否有跳过注解
        /// <br/> <see cref="GameIgnoreAttribute"//>
        /// </summary>
        /// <returns></returns>
        bool IsIgnore(INamedTypeSymbol symbol)
        {
            var gameCallAttribute = symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass) == typeof(GameIgnoreAttribute).FullName).FirstOrDefault();
            return gameCallAttribute != null;
        }

        public void AnalyzeClass(CompileContext context, SemanticModel semanticModel, INamedTypeSymbol classSymbol)
        {

            if (IsIgnore(classSymbol))
                return;

            LClass lClass = context.CreateClass(classSymbol);
            Dictionary<string, IMethodSymbol> functions = new Dictionary<string, IMethodSymbol>();
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                {// 字段
                    lClass.Connects.Add(field.Name);
                }
                else if (member is IMethodSymbol method)
                {// 方法
                    functions.Add(method.Name, method);
                }
                else
                {
                    throw new Exception("不支持的类成员类型" + member);
                }
            }

            // TODO 将校验移到合适位置
            //IMethodSymbol? mainFunction = functions.GetValueOrDefault(nameof(GameCPU.Main));
            //if (mainFunction == null)
            //    throw new Exception($"没有找到[{classSymbol}]的主函数[{nameof(GameCPU.Main)}]，请检查代码是否正确");

            context.MethodAnalyzer.AnalyzeMethods(context, functions, semanticModel, lClass);


            //// 分析方法
            //foreach ((string key, MethodDeclarationSyntax methodDeclaration) in functions)
            //{
            //    AnalyzeFunction(context, classDeclaration, methodDeclaration, lClass);
            //}

            //// 计算可达性 由于裁剪无效代码
            //SortedSet<string> visitedFunctions = new SortedSet<string>();
            //visitedFunctions.Add(nameof(GameCPU));

            Console.WriteLine(1);

        }

        public void AnalyzeCPUClass(CompileContext context, INamedTypeSymbol classSymbol, LClass @class)
        {
            if (!IsCPUClass(classSymbol))
                return;
            foreach ((var methodSymbol, var method) in @class.Functions)
            {
                if (methodSymbol.Name == nameof(GameCPU.Main))
                {
                    context.MethodAnalyzer.AnalyzeMethodBody(context, methodSymbol, method, true);
                }
            }
        }

    }
    internal class MethodAnalyzer : BaseAnalyzer
    {
        /// <summary>
        /// 分析方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="functions"></param>
        /// <param name="semanticModel"></param>
        /// <param name="lClass"></param>
        public void AnalyzeMethods(CompileContext context, Dictionary<string, IMethodSymbol> functions, SemanticModel semanticModel, LClass lClass)
        {
            foreach (var (name, func) in functions)
            {
                AnalyzeMethod(context, semanticModel, lClass, func);

            }
        }

        public void AnalyzeMethodBody(CompileContext context, IMethodSymbol methodSymbol, LMethod method, bool checkCircularDependency)
        {
            if (checkCircularDependency && context.Analyzing.ContainsKey(methodSymbol))
                throw new Exception("循环依赖:" + methodSymbol.ToString());

            if (method.Block != null)
                return;
            context.Analyzing.Add(methodSymbol, method);
            try
            {
                MethodDeclarationSyntax methodSyntax = (MethodDeclarationSyntax)
                    methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var statements = methodSyntax!.Body!.Statements;
                var semanticModel = context.SemanticModels[methodSyntax.SyntaxTree];
                context.MethodBodyAnalyzer.Analyze(context, method, semanticModel, statements.ToList());
            }
            finally
            {
                context.Analyzing.Remove(methodSymbol);
            }
        }

        /// <summary>
        /// 分析方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="semanticModel"></param>
        /// <param name="method"></param>
        /// <param name="funcSyntax"></param>
        LMethod? AnalyzeMethod(CompileContext context, SemanticModel semanticModel, LClass @class, IMethodSymbol methodSymbol)
        {
            if (methodSymbol.DeclaringSyntaxReferences.Length > 1)
                throw new Exception("不支持 partial 方法:" + methodSymbol.ToString());

            if (methodSymbol.DeclaringSyntaxReferences.Length == 0)
            {
                Console.WriteLine("函数被忽略" + methodSymbol);
                return null;
            }

            var method = @class.CreateMethod(methodSymbol);
            method.Parameters = AnalyzeMethodParameters(context, semanticModel, methodSymbol, method);

            // 分析调用类型
            AnalyzeCustomerCallAttribute(method, methodSymbol);

            return method;
        }

        private List<LParameter> AnalyzeMethodParameters(CompileContext context, SemanticModel semanticModel, IMethodSymbol methodSymbol, LMethod method)
        {
            List<LParameter> res = new List<LParameter>();
            foreach (var parameterSymbol in methodSymbol.Parameters)
            {
                var v = method.VariableTable.Add(parameterSymbol.Type, parameterSymbol, parameterSymbol.Name);
                LParameter parameter = new LParameter(v, parameterSymbol.HasExplicitDefaultValue ? parameterSymbol.ExplicitDefaultValue : null);
                res.Add(parameter);
            }
            return res;
        }

        /// <summary>
        /// 分析 方法类型
        /// <br/> <see cref="CustomerCallAttribute"/>
        /// </summary>
        /// <param name="method"></param>
        private void AnalyzeCustomerCallAttribute(LMethod method, IMethodSymbol symbol)
        {
            var gameCallAttribute = symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass) == typeof(CustomerCallAttribute).FullName).FirstOrDefault();
            if (gameCallAttribute != null)
            {
                method.CallMode = (MethodCallMode)(int)gameCallAttribute.ConstructorArguments[0].Value;
            }
            else
            {
                method.CallMode = MethodCallMode.Default;
            }
        }



        // 已经在分析方法体时分析
        ///// <summary>
        ///// 分析方法中所有本地变量
        ///// </summary>
        ///// <param name="semanticModel"></param>
        ///// <param name="methods"></param>
        ///// <param name="funcSyntax"></param>
        //private void AnalyzeVariables(SemanticModel semanticModel, Method methods, MethodDeclarationSyntax funcSyntax)
        //{
        //    var localVariables = funcSyntax.DescendantNodes().OfType<LocalDeclarationStatementSyntax>();
        //    foreach (var localVariable in localVariables)
        //    {
        //        var type = localVariable.Declaration.Type;
        //        var typeInfo = semanticModel.GetTypeInfo(type);
        //        foreach (var variable in localVariable.Declaration.Variables)
        //        {
        //            //var a = semanticModel.GetTypeInfo(funcSyntax.DescendantNodes().OfType<IdentifierNameSyntax>().ToList()[0]);
        //            methods.VariableTable.Add(typeInfo, variable.Identifier.ToString());
        //        }
        //    }
        //}

    }
}
