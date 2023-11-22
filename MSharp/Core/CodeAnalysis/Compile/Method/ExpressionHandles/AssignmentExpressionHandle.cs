using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using System;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    internal class AssignmentExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(AssignmentExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is AssignmentExpressionSyntax);
            AssignmentExpressionSyntax aes = (AssignmentExpressionSyntax)syntax;
            var right = GetRight(p.WithExpression(aes.Right));
            return Assign(p.WithExpression(aes.Left).WithRight(right));
        }
    }

}
