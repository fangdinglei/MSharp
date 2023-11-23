using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Game;
using MSharp.Core.Shared;
using System;
using System.Diagnostics;
using System.Reflection;
using static MSharp.Core.Compile.TypeUtility;

namespace MSharp.Core.Compile.Method.ExpressionHandles
{
    internal class MemberAccessExpressionHandle : ExpressionHandle
    {
        public override Type Syntax => typeof(MemberAccessExpressionSyntax);
        public override LVariableOrValue DoGetRight(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is MemberAccessExpressionSyntax);
            MemberAccessExpressionSyntax maes = (MemberAccessExpressionSyntax)syntax;


            var right = maes.Name.Identifier.Value;
            var symbol = p.SemanticMode.GetSymbolInfo(maes.Name).Symbol!;
            if (symbol.Kind == SymbolKind.Method)
            {
                return null;
                //return new LVariableOrValue(new LVariable);
            }
            else
            {
                if (CheckGameObjectData(maes.Expression, maes.Name, context, semanticModel))
                {
                    var left = GetRight(p.WithExpression(maes.Expression));
                    Debug.Assert(left.IsVariable, "member access:obj value must be variable");
                    LVariableOrValue sensorVar = new LVariableOrValue(method.VariableTable.AddTempVariable(semanticModel.GetTypeInfo(maes).Type!));
                    p.Block.Emit(new Code_Sensor(sensorVar, left, "@" + (string)right!));
                    return sensorVar;

                }
                else if (CheckGameConst(p, maes, (string)right!, out var @const))
                {
                    return @const!;
                }
                else
                {
                    throw new Exception($"oop not allowed or Const class not registered in {nameof(MemberAccessExpressionHandle)}.{nameof(CheckGameConst)}");
                }
            }
        }
        public override LVariableOrValue DoAssign(Parameter p)
        {
            var syntax = p.Syntax;
            var semanticModel = p.SemanticMode;
            var method = p.Method;
            var context = p.Context;
            Debug.Assert(syntax is MemberAccessExpressionSyntax);
            MemberAccessExpressionSyntax maes = (MemberAccessExpressionSyntax)syntax;

            var obj = GetRight(p.WithExpression(maes.Expression));
            var memberName = maes.Name.Identifier.Value! as string;
            Debug.Assert(obj.IsVariable, "member access:left value must be variable");

            throw new Exception("oop not allowed");

            //if (!CheckGameObjectData(maes.Expression, maes.Name, context, semanticModel))
            //    throw new Exception("oop not allowed");

            //// C # setter prevents modification of read-only game data
            //if (p.Context.TypeUtility.IsSonOf(obj.Variable!.Type, typeof(Building)))
            //{
            //    // building control
            //    p.Block.Emit(new Code_Control(obj, memberName!, p.Right!));
            //}
            //else if (p.Context.TypeUtility.IsSonOf(obj.Variable!.Type, typeof(Unit)))
            //{
            //    // unit control
            //    p.Block.Emit(new Code_UnitControl(memberName!, p.Right!));

            //}
            //else
            //{
            //    throw new Exception("oop not allowed");
            //}


            return p.Right!;


        }
        private bool CheckGameObjectData(ExpressionSyntax left, SimpleNameSyntax right, CompileContext context, SemanticModel semanticModel)
        {
            if (left is not IdentifierNameSyntax)
                return false;
            var type = semanticModel.GetTypeInfo(left).Type!;
            if (IsSonOf(type, typeof(GameObject))
                && HasAttribute(semanticModel.GetSymbolInfo(right).Symbol!, typeof(GameObjectDataAttribute)))
            {
                return true;
            }
            return false;
        }

        private bool CheckGameConst(Parameter p, MemberAccessExpressionSyntax maes, string name, out LVariableOrValue? res)
        {
            var fullName = GetFullName(p.SemanticMode.GetTypeInfo(maes).Type!);
            FieldInfo? field = null;
            if (fullName == typeof(UnitConst).FullName && (field = typeof(UnitConst).GetField(name)) != null) { }
            else if (fullName == typeof(ItemConst).FullName && (field = typeof(ItemConst).GetField(name)) != null) { }
            else if (fullName == typeof(LiquidConst).FullName && (field = typeof(LiquidConst).GetField(name)) != null) { }

            if (field != null)
            {
                var gameConst = (GameConst)field!.GetValue(null)!;
                res = new LVariableOrValue("@" + gameConst.Name);
                return true;
            }
            res = null;
            return false;
        }

    }

}
