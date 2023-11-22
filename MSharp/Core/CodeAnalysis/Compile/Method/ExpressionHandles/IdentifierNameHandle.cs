using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Language;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    /// <summary>
    /// Variable 变量取值
    /// </summary>
    internal class IdentifierNameHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(IdentifierNameSyntax);

        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            Debug.Assert(syntax is IdentifierNameSyntax);
            var lns = (IdentifierNameSyntax)syntax;
            LVariable? res;
            var symbol = semanticModel.GetSymbolInfo(lns).Symbol!;
            //本地变量
            method.VariableTable.TryGet(symbol, out res);
            // 类成员
            if (res == null)
                method.Parent.VariableTable.TryGet(symbol, out res);
            if (res == null)
                throw new Exception($"define of {symbol.Name} is not supported");
            return new LVariableOrValue(res);
        }

        public override LVariableOrValue DoAssign(Parameter p)
        {
            // obtain the variable that needs to be assigned
            var left = DoGetRight(p).Variable!;
            p.Method.Block!.Emit(new Code_Assign(left, p.Right!));
            return new LVariableOrValue(left);
        }
    }

}
