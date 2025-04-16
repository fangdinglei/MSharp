using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.Compile.Method.ExpressionHandles
{
    internal class TupleExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(TupleExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            Debug.Assert(syntax is TupleExpressionSyntax);
            TupleExpressionSyntax tes = (TupleExpressionSyntax)syntax;

            var res = new List<object>();
            foreach (var item in tes.Arguments)
                res.Add(GetRight(p.WithExpression(item.Expression)));
            return new LVariableOrValue(res);
        }
    }

}
