using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile.Method.StatementHandles;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile
{
    public class CompileContext
    {
        internal readonly StatementManager StatementManager = new();

        internal readonly Dictionary<INamedTypeSymbol, LClass> Classes = new(SymbolEqualityComparer.Default);

        internal readonly Dictionary<IMethodSymbol, LMethod> Methods = new(SymbolEqualityComparer.Default);

        internal readonly Dictionary<IMethodSymbol, LMethod> Analyzing = new(SymbolEqualityComparer.Default);

        internal readonly Dictionary<IMethodSymbol, LMethod> WaitFurtherAnalyzing = new(SymbolEqualityComparer.Default);

        internal readonly Dictionary<SyntaxTree, SemanticModel> SemanticModels;

        public CompileContext(Dictionary<SyntaxTree, SemanticModel> semanticModels)
        {
            SemanticModels = semanticModels;
        }

        internal LClass CreateClass(INamedTypeSymbol symbol)
        {
            Debug.Assert(symbol != null && symbol.TypeKind == TypeKind.Class, $"{nameof(CompileContext)}:{nameof(CreateClass)}:{nameof(symbol)} must be a class symbol");
            var res = new LClass(this, symbol);
            Classes.Add(symbol, res);
            return res;
        }

        internal LMethod AddMethod(IMethodSymbol symbol, LMethod method)
        {
            Methods.Add(symbol, method);
            return method;
        }


    }
}
