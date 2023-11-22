using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using System;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// () Parenthesized Expression
    /// <br/>括号语句，直接让处理子项
    /// </summary>
    internal class ParenthesizedExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(ParenthesizedExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is ParenthesizedExpressionSyntax);
            return GetRight(p.WithExpression(((ParenthesizedExpressionSyntax)syntax).Expression));
        }

        public override LVariableOrValue DoAssign(Parameter p)
        {
            return DoGetRight(p);
        }
    }

}
