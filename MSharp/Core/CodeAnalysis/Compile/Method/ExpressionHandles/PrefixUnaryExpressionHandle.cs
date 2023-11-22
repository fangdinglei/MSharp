using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// -1  -1.4 etc (negative numbers is not literal in C#)
    /// <br/>单目运算，如负数（负数应该也是字面量，但是C#给的是单目运算+字面量）
    /// </summary>
    internal class PrefixUnaryExpressionHandle : ExpressionHandle
    {
        Dictionary<string, MindustryOperatorKind> UnaryOperatorMap = new()
        {
            { "-", MindustryOperatorKind.sub},
        };
        public override Type Syntax => typeof(PrefixUnaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is PrefixUnaryExpressionSyntax);
            var pues = (PrefixUnaryExpressionSyntax)syntax;
            if (pues.OperatorToken.Text == "-")
            {
                if (pues.Operand is LiteralExpressionSyntax les)
                {
                    var v = les.Token.Value;
                    // INumber may help
                    if (v is int a)
                        return new LVariableOrValue(-a);
                    else if (v is uint b)
                        return new LVariableOrValue(-b);
                    else if (v is long c)
                        return new LVariableOrValue(-c);
                    else if (v is ulong d)
                        throw new Exception(" - is not allowed to operate ulong");
                    else if (v is short e)
                        return new LVariableOrValue(-e);
                    else if (v is ushort f)
                        return new LVariableOrValue(-f);
                    else if (v is byte g)
                        return new LVariableOrValue(-g);
                    else if (v is char h)
                        return new LVariableOrValue(-h);
                    else if (v is double i)
                        return new LVariableOrValue(-i);
                    else if (v is float j)
                        return new LVariableOrValue(-j);
                    else
                        throw new Exception(" - is not allowed to operate " + v?.GetType().FullName ?? "null");
                }
            }
            else if (pues.OperatorToken.Text == "+")
            {
                if (pues.Operand is LiteralExpressionSyntax les)
                {
                    return new LVariableOrValue(les.Token.Value);
                }
            }
            else if (pues.OperatorToken.Text == "++")
            {
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
             *  ++i
             *  
             *  op add var2 i 1
             *  set i var2
             *  
             *  ++block1.@a
             *  sensor var2 block1 @a
             *  op add var3 var2 1
             *  control xxx
             */
            var right = GetRight(p);
            var var2 = new LVariableOrValue(p.Block.Method.VariableTable.AddTempVariable(right.Variable!.Type!));
            p.Block.Emit(new Code_Operation(MindustryOperatorKind.add, var2, right, new LVariableOrValue(1)));
            Assign(p.WithRight(var2));
            return var2;
        }

    }

}
