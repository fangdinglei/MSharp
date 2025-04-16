using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using System;
using System.Diagnostics;

namespace MSharp.Core.Compile.Method.ExpressionHandles
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

            if (pues.OperatorToken.Text == "++")
            {
                // i ++
                return ProcessSelfIncreasingAndDecreasing(p.WithExpression(pues.Operand), MindustryOperatorKind.add);
            }
            else if (pues.OperatorToken.Text == "--")
            {
                return ProcessSelfIncreasingAndDecreasing(p.WithExpression(pues.Operand), MindustryOperatorKind.sub);
            }
            throw new Exception();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        static public LVariableOrValue ProcessSelfIncreasingAndDecreasing(Parameter p, MindustryOperatorKind op)
        {
            /**
             *  this operation will be rewritten during the optimization phase
             *  i++ i--
             */
            var right = GetRight(p);
            var var2 = new LVariableOrValue(p.Block.Method.VariableTable.AddTempVariable(right.Variable!.Type!));
            p.Block.Emit(new Code_Operation(MindustryOperatorKind.add, var2, right, new LVariableOrValue(1)), true);
            Assign(right.Variable!, var2, p.Block, true);
            return right;
        }

    }

}
