using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    internal class PostExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(PostfixUnaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is PostfixUnaryExpressionSyntax);
            var pues = (PostfixUnaryExpressionSyntax)syntax;

            var right = GetRight(p.WithExpression(pues.Operand));

            if (pues.OperatorToken.Text == "++")
            {
                var t = p.Block.Codes;
                p.Block.Codes = new List<BaseCode>();

                var res = PrefixUnaryExpressionHandle.ProcessSelfIncreasingAndDecreasing(p.WithExpression(pues.Operand), MindustryOperatorKind.add);

                p.Block.PostCodes.InsertRange(0, p.Block.Codes);
                p.Block.Codes = t;
                return res;
            }
            else if (pues.OperatorToken.Text == "--")
            {
                var t = p.Block.Codes;
                p.Block.Codes = new List<BaseCode>();

                var res = PrefixUnaryExpressionHandle.ProcessSelfIncreasingAndDecreasing(p.WithExpression(pues.Operand), MindustryOperatorKind.sub);

                p.Block.PostCodes.InsertRange(0, p.Block.Codes);
                p.Block.Codes = t;
                return res;
            }
            throw new Exception();
        }
    }

}
