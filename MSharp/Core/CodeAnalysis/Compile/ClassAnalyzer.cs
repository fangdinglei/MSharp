using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile.Method;
using MSharp.Core.Exceptions;
using MSharp.Core.Logic;
using MSharp.Core.Shared;
using System;
using System.Linq;
using static MSharp.Core.CodeAnalysis.Compile.TypeUtility;

namespace MSharp.Core.CodeAnalysis.Compile
{
    static internal class ClassAnalyzer
    {

        /// <summary>
        /// is class needs to ignore
        /// <br/> <see cref="GameIgnoreAttribute"//>
        /// </summary>
        /// <returns></returns>
        static private bool IsIgnore(INamedTypeSymbol symbol)
        {
            var gameCallAttribute = symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass!) == typeof(GameIgnoreAttribute).FullName).FirstOrDefault();
            return gameCallAttribute != null;
        }

        /// <summary>
        /// analyze field,method(class first analyze)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="semanticModel"></param>
        /// <param name="classSymbol"></param>
        /// <exception cref="Exception"></exception>
        static public void Analyze(CompileContext context, SemanticModel semanticModel, INamedTypeSymbol classSymbol)
        {

            if (IsIgnore(classSymbol))
                return;

            LClass lClass = context.CreateClass(classSymbol);
            foreach (var member in classSymbol.GetMembers())
            {
                if (member is IFieldSymbol field)
                {// 字段
                    lClass.VariableTable.Add(field, field.Type, field.Name);
                }
                else if (member is IMethodSymbol method)
                {// 方法
                    MethodAnalyzer.AnalyzeDeclaration(context, method, semanticModel, lClass);
                }
                else
                {
                    throw new CompileError($"not supported class member{member.Kind}");
                }
            }


        }

        /// <summary>
        /// is cpu code
        /// </summary>
        /// <param name="classSymbol"></param>
        /// <returns></returns>
        static private bool IsCPUClass(INamedTypeSymbol classSymbol)
        {
            if (classSymbol.IsAbstract)
                return false;
            return IsSonOf(classSymbol, typeof(GameCPU));
        }

        /// <summary>
        /// analyze cpu class(class second analyze)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="classSymbol"></param>
        /// <param name="class"></param>
        static public void AnalyzeCPU(CompileContext context, INamedTypeSymbol classSymbol, LClass @class)
        {
            if (!IsCPUClass(classSymbol))
                return;
            foreach ((var methodSymbol, var method) in @class.Functions)
            {
                if (methodSymbol.Name == nameof(GameCPU.Main))
                {
                    MethodAnalyzer.AnalyzeBody(context, methodSymbol, method, true);
                }
            }
        }

    }
}
