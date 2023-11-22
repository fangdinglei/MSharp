﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using System;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    internal class CastExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(CastExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is CastExpressionSyntax);
            CastExpressionSyntax ces = (CastExpressionSyntax)syntax;
            return GetRight(p.WithExpression(ces.Expression));
        }
    }

}
