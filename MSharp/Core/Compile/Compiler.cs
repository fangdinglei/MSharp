using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Exceptions;
using MSharp.Core.Simulate;
using MSharp.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSharp.Core.Compile
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
            // analyze class field,method
            foreach (var task in tasks)
            {
                ClassAnalyzer.Analyze(analyzeContext, task.Item1, task.Item2);
            }
            List<LMethod> mainMethods = new();
            // analyze cpu code
            foreach (var task in tasks)
            {
                if (analyzeContext.Classes.TryGetValue(task.Item2, out var c))
                {
                    LMethod? mainMethod = ClassAnalyzer.AnalyzeCPU(analyzeContext, task.Item2, c);
                    mainMethods.AddIfNotNull(mainMethod);
                }
            }

            // 暂时只支持一个结果
            if (mainMethods.Count == 0)
                throw new CompileError("no cpu class found");
            if (mainMethods.Count > 1)
                throw new CompileError("only one cpu class supported now");

            List<string> res = new List<string>();
            var intermediateCodes = mainMethods[0].Block!.Codes;
            int idx = 0;
            foreach (var code in intermediateCodes)
            {
                if (code.Deprecated)
                    continue;
                code.Index = idx;
                idx += code.CodeLength;
            }

            MVM mvm = new MVM(intermediateCodes);
            mvm.Run();
            return intermediateCodes;
        }

        public string CompileToText(params CodeFile[] codes)
        {
            var intermediateCodes = Compile(codes);
            StringBuilder sb = new StringBuilder();
            foreach (var code in intermediateCodes)
            {
                sb.AppendLine(code.ToMindustryCodeString());
            }
            return sb.ToString();
        }

    }

}
