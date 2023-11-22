using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// + - *, etc 二元运算
    /// </summary>
    internal class BinaryExpressionHandle : ExpressionHandle
    {
        Dictionary<string, MindustryOperatorKind> BinaryOperatorMap = new()
        {
            { "+", MindustryOperatorKind.add},
            { "-", MindustryOperatorKind.sub},
            { "*", MindustryOperatorKind.mul},
            { "/", MindustryOperatorKind.div},
            { "==", MindustryOperatorKind.E},
            { "<", MindustryOperatorKind.LT},
            { "<=", MindustryOperatorKind.LE},
            { ">", MindustryOperatorKind.GT},
            { ">=", MindustryOperatorKind.GE},
            { "%", MindustryOperatorKind.mod},
            { ">>", MindustryOperatorKind.shr},
            { "<<", MindustryOperatorKind.shl},
            { "|", MindustryOperatorKind.or},
            { "&", MindustryOperatorKind.and},
            { "^", MindustryOperatorKind.xor},
            { "~", MindustryOperatorKind.not},
        };
        public override Type Syntax => typeof(BinaryExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            Debug.Assert(syntax is BinaryExpressionSyntax);
            var bes = (BinaryExpressionSyntax)syntax;
            var typeInfo = semanticModel.GetTypeInfo(bes);
            Debug.Assert(typeInfo.Type != null);
            var variable = new LVariableOrValue(method.VariableTable.AddTempVariable(typeInfo.Type));
            var kind = BinaryOperatorMap[bes.OperatorToken.Text];
            var left = GetRight(p.WithExpression(bes.Left));
            var right = GetRight(p.WithExpression(bes.Right));
            p.Block.Emit(new Code_Operation(kind, variable, left, right));
            return variable;
        }
    }

}
