using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.Method;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSharp.Core.CodeAnalysis.Compile
{
    internal class Compiler : ICompiler
    {
        public Compiler()
        {



        }



        public List<BaseCode> Compile(params string[] codes)
        {
            // 解析C#代码
            var syntaxTrees = codes.Select(code =>
            {
                return CSharpSyntaxTree.ParseText(code);
            }).ToArray();
            var compilationUnitSyntaxes = syntaxTrees.Select(it => it.GetCompilationUnitRoot()).ToArray();
            var compilation = CSharpCompilation.Create("fangdinglei")
                .AddSyntaxTrees(syntaxTrees)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location)
                )
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary));
            // 输出编译错误
            compilation.GetDeclarationDiagnostics().ToList().ForEach(Console.WriteLine);
            var semanticModels = syntaxTrees.Select(it =>
            {
                var semanticModel = compilation.GetSemanticModel(it);
                if (semanticModel == null)
                    throw new Exception("GetSemanticModel:未知错误 请检查C#语法");
                return semanticModel;
            }).ToDictionary(it => it.SyntaxTree);


            CompileContext analyzeContext = new CompileContext(semanticModels);
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = semanticModels[syntaxTree];
                var classes = syntaxTree.GetCompilationUnitRoot()
                    .DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                // 分析每个类
                foreach (var clazz in classes)
                {
                    INamedTypeSymbol? classSymbol = semanticModel.GetDeclaredSymbol(clazz);
                    if (classSymbol == null)
                        throw new Exception("GetDeclaredSymbol:未知错误 请检查C#语法");
                    new ClassAnalyzer().AnalyzeClass(analyzeContext, semanticModel, classSymbol);
                }
            }
            foreach (var syntaxTree in syntaxTrees)
            {
                var semanticModel = semanticModels[syntaxTree];
                var classes = syntaxTree.GetCompilationUnitRoot()
                    .DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                // 分析每个类
                foreach (var clazz in classes)
                {
                    INamedTypeSymbol? classSymbol = semanticModel.GetDeclaredSymbol(clazz);
                    new ClassAnalyzer().AnalyzeCPUClass(analyzeContext, classSymbol, analyzeContext.Classes[classSymbol]);
                }
            }


            return null;

        }

        public string CompileToText(params string[] codes)
        {
            throw new NotImplementedException();
        }
    }


    public class T : CSharpSyntaxWalker
    {

    }
}
