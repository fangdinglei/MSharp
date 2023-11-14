using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace MSharp.Core.CodeAnalysis.Compile.Method
{
    internal class MethodBodyAnalyzer : BaseAnalyzer
    {
        public LBlock Analyze(CompileContext context, LMethod method, SemanticModel semanticModel, List<StatementSyntax> syntaxes)
        {
            var block = new LBlock(method);
            method.Block = block;
            foreach (var statement in syntaxes)
            {
                context.StatementManager.Handle(statement, context, semanticModel, block);
            }
            return block;
        }

    }
}