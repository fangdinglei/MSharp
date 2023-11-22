using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Exceptions;
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

        public List<BaseCode> Compile(params CodeFile[] codes)
        {
            // Parse Code
            var syntaxTrees = codes.Select(code =>
            {
                return CSharpSyntaxTree.ParseText(code.Code);
            }).ToArray();
            var compilation = CSharpCompilation.Create("fangdinglei")
                .AddSyntaxTrees(syntaxTrees)
                .AddReferences(
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(List<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location)
                )
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            // 输出编译错误
            compilation.GetDeclarationDiagnostics().ToList().ForEach(Console.WriteLine);
            var semanticModels = syntaxTrees.Select(it =>
            {
                var semanticModel = compilation.GetSemanticModel(it);
                if (semanticModel == null)
                    throw new CompileError("GetSemanticModel returns null, please check your code");
                return semanticModel;
            }).ToDictionary(it => it.SyntaxTree);


            CompileContext analyzeContext = new CompileContext(semanticModels);
            var tasks = syntaxTrees.Aggregate(new List<(SemanticModel, INamedTypeSymbol)>(), (res, syntaxTree) =>
            {
                var semanticModel = semanticModels[syntaxTree];
                var classes = syntaxTree.GetCompilationUnitRoot()
                    .DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                res.AddRange(classes.Select(it => (semanticModel, semanticModel.GetDeclaredSymbol(it)!)));
                return res;
            });
            // analyze field,method
            foreach (var task in tasks)
            {
                ClassAnalyzer.Analyze(analyzeContext, task.Item1, task.Item2);
            }
            // analyze cpu code
            foreach (var task in tasks)
            {
                if (analyzeContext.Classes.TryGetValue(task.Item2, out var c))
                    ClassAnalyzer.AnalyzeCPU(analyzeContext, task.Item2, c);
            }


            return null;

        }

        public string CompileToText(params CodeFile[] codes)
        {
            throw new NotImplementedException();
        }

    }
}
