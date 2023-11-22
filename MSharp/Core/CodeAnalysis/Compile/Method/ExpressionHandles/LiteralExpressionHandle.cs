using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using System;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// 字面量
    /// </summary>
    internal class LiteralExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(LiteralExpressionSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is LiteralExpressionSyntax);
            var les = (LiteralExpressionSyntax)syntax;
            return new LVariableOrValue(les.Token.Value);
        }
    }

}
