using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using System;
using System.Diagnostics;
using System.Reflection;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    internal class DeclarationExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(DeclarationExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is DeclarationExpressionSyntax);
            var les = (DeclarationExpressionSyntax)syntax;
            var symbol = p.SemanticMode.GetSymbolInfo(les).Symbol!;
            var type = p.SemanticMode.GetTypeInfo(les).Type!;
            if (les.Designation is SingleVariableDesignationSyntax svds)
            {
                return new LVariableOrValue(p.Method.VariableTable.AddLocalVariable(type, symbol, (string)svds.Identifier.Value!));
            }

            throw new Exception();

        }
    }

}
