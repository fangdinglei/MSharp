using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.MindustryCode;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.Method.ExpressionHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSharp.Core.Compile.Method.StatementHandles
{
    internal struct StatementHandleParameters
    {
        public readonly CompileContext Context;
        public readonly SemanticModel SemanticModel;
        public readonly LBlock Block;
        public readonly StatementSyntax Syntax;

        public StatementHandleParameters(CompileContext context, SemanticModel semanticModel,
            LBlock block, StatementSyntax syntax)
        {
            Context = context;
            SemanticModel = semanticModel;
            Block = block;
            Syntax = syntax;
        }

        public StatementHandleParameters With(StatementSyntax syntax)
        {
            return new StatementHandleParameters(Context, SemanticModel, Block, syntax);
        }

        public StatementHandleParameters With(LBlock block)
        {
            return new StatementHandleParameters(Context, SemanticModel, block, Syntax);
        }
    }

    internal abstract class StatementHandle
    {
        static Dictionary<Type, StatementHandle> _handles = new();

        static StatementHandle()
        {
            // 初始化
            typeof(StatementHandle).Assembly.GetTypes()
                .Where(it =>
                {
                    Type? ptr = it;
                    while (true)
                    {
                        if (ptr == null || ptr == typeof(object))
                            return false;
                        if (ptr.BaseType == typeof(StatementHandle))
                            return true;
                        ptr = ptr.BaseType;
                    }
                })
                .Select(it =>
                {
                    var res = (StatementHandle?)Activator.CreateInstance(it)!;
                    return res;
                })
                .ToList().ForEach(it =>
                {
                    Debug.Assert(it != null);
                    foreach (Type type in it.Types)
                    {
                        _handles.Add(type, it);
                    }
                });
        }
        static public void Handle(StatementSyntax statement, CompileContext context, SemanticModel semanticModel, LBlock block)
        {
            Type? type = null;
            type = statement.GetType();
            if (!_handles.TryGetValue(type, out var handle))
            {
                Console.WriteLine("语义分析：未知的语句类型\n" + statement);
                return;
            }
            handle.DoHandle(new StatementHandleParameters(context, semanticModel, block, statement));
            block.MergePostCodes();
        }


        public abstract List<Type> Types { get; }

        public abstract void DoHandle(StatementHandleParameters parameters);

        static protected StatementSyntax[] GetStatements(StatementSyntax ss)
        {
            StatementSyntax[] statements;
            if (ss is BlockSyntax bs)
            {
                // while(){xxx}
                // if(){xxx}
                // ...
                statements = bs.Statements.ToArray();
            }
            else
            {
                // while() xxx
                // if() xxx
                // ...
                Debug.Assert(ss is ExpressionStatementSyntax);
                ExpressionStatementSyntax ess = (ExpressionStatementSyntax)ss;
                statements = new StatementSyntax[] { ess };
            }
            return statements;
        }
        static protected LBlock HandleBlock(StatementHandleParameters p)
        {
            var block = new LBlock(p.Block.Method);
            StatementSyntax[] statements = GetStatements(p.Syntax);

            foreach (var item in statements)
                Handle(item, p.Context, p.SemanticModel, block);
            return block;
        }

    }

    internal class LocalVariableStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() { typeof(LocalDeclarationStatementSyntax) };

        public override void DoHandle(StatementHandleParameters p)
        {
            var localDeclaration = (LocalDeclarationStatementSyntax)p.Syntax;
            var type = localDeclaration.Declaration.Type;
            var typeInfo = p.SemanticModel.GetTypeInfo(type);
            foreach (var variable in localDeclaration.Declaration.Variables)
            {
                var symbol = p.SemanticModel.GetDeclaredSymbol(variable);
                var var2 = p.Block.Method.VariableTable.AddLocalVariable(typeInfo.Type!, symbol!, variable.Identifier.ToString());
                if (variable.Initializer != null)
                {
                    var p2 = new ExpressionHandle.Parameter(variable.Initializer!.Value, p.Context, p.SemanticModel, p.Block, p.Block.Method);
                    var initVal = ExpressionHandle.GetRight(p2);
                    ExpressionHandle.Assign(var2, initVal, p.Block);
                }
            }
        }
    }

    internal class ExpressionStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(ExpressionStatementSyntax) ,
        };

        public override void DoHandle(StatementHandleParameters p)
        {
            ExpressionStatementSyntax ess = (ExpressionStatementSyntax)p.Syntax;
            ExpressionHandle.GetRight(new(ess.Expression, p.Context, p.SemanticModel, p.Block, p.Block.Method));
        }
    }

    internal class IfStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(IfStatementSyntax) ,
        };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        public override void DoHandle(StatementHandleParameters p)
        {
            IfStatementSyntax iss = (IfStatementSyntax)p.Syntax;
            var condition = ExpressionHandle.GetRightAndMergePostCode(new(iss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));

            var ifBlock = HandleBlock(p.With(iss.Statement));

            // jump to else/else if/next
            p.Block.Emit(new Code_Jump(out var jumpNext, Code_Jump.OpCode.notEqual, condition, LVariableOrValue.ONE));
            // if body
            p.Block.Emit(ifBlock.Codes);

            p.Block.ReturnCall += (node) => ifBlock.ReturnCall(node);
            p.Block.ContinueCall += (node) => ifBlock.ContinueCall(node);

            if (iss.Else != null)
            {
                // else/else if
                var elseStm = iss.Else.Statement;
                LBlock elseBlock;
                if (elseStm is IfStatementSyntax)
                {
                    elseBlock = new LBlock(p.Block.Method);
                    DoHandle(p.With(elseBlock).With(elseStm));
                }
                else
                {
                    elseBlock = HandleBlock(p.With(elseStm));
                }


                if (elseBlock.Codes.Count() != 0)
                {
                    // end of if : jump out
                    p.Block.Emit(new Code_Jump(out var jumpOut, Code_Jump.OpCode.always));
                    // jump here when if is not true
                    jumpNext.JumpTo(elseBlock.Codes[0]);
                    // else if body
                    p.Block.Emit(elseBlock.Codes);

                    p.Block.ReturnCall += (node) => elseBlock.ReturnCall(node);
                    p.Block.ContinueCall += (node) => elseBlock.ContinueCall(node);
                    p.Block.NextCall += (node) => elseBlock.NextCall(node);
                    // end of if : jump out
                    p.Block.NextCall += (node) => jumpOut.JumpTo(node);
                }

            }
            // if no else or else is empty,set jump next
            if (jumpNext.To == null)
            {
                p.Block.NextCall += (node) => jumpNext.JumpTo(node);
            }

        }
    }

    internal class WhileStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(WhileStatementSyntax) ,
        };

        public override void DoHandle(StatementHandleParameters p)
        {
            WhileStatementSyntax wss = (WhileStatementSyntax)p.Syntax;


            var conditionCodeStart = p.Block.Codes.Count;
            var condition = ExpressionHandle.GetRightAndMergePostCode(new(wss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));

            var whileBlock = HandleBlock(p.With(wss.Statement));

            if (whileBlock.Codes.Count > 0)
            {
                // jump out of while
                p.Block.Emit(new Code_Jump(out var jumpOut, Code_Jump.OpCode.notEqual, condition, LVariableOrValue.ONE));
                // while body
                p.Block.Emit(whileBlock.Codes);
                // jump begin of while
                p.Block.Emit(new Code_Jump(out var jumpBegin, Code_Jump.OpCode.always));
                // continue in block  or  end of while should jump to begin
                jumpBegin.JumpTo(p.Block.Codes[conditionCodeStart]);
                whileBlock.ContinueCall(jumpBegin.To);
                // next of while block is jumpBegin   jumpBegin => jumpBegin.To
                whileBlock.NextCall(jumpBegin.To);
                p.Block.ReturnCall += (node) => whileBlock.ReturnCall(node);

                // register jump out
                p.Block.NextCall += (node) => jumpOut.JumpTo(node);
            }
        }
    }

    internal class ForStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(ForStatementSyntax) ,
        };

        public override void DoHandle(StatementHandleParameters p)
        {
            ForStatementSyntax fss = (ForStatementSyntax)p.Syntax;

            if (fss.Declaration != null)
            {
                var type = p.SemanticModel.GetTypeInfo(fss.Declaration.Type).Type!;
                foreach (var item in fss.Declaration.Variables)
                {
                    var symbol = p.SemanticModel.GetDeclaredSymbol(item)!;
                    var var2 = p.Block.Method.VariableTable.AddLocalVariable(type, symbol, (string)item.Identifier.Value!);
                    if (item.Initializer != null)
                    {
                        var p2 = new ExpressionHandle.Parameter(item.Initializer!.Value, p.Context, p.SemanticModel, p.Block, p.Block.Method);
                        var initVal = ExpressionHandle.GetRight(p2);
                        ExpressionHandle.Assign(var2, initVal, p.Block);
                    }
                }
            }

            var conditionCodeStart = p.Block.Codes.Count;
            Code_Jump? jumpOut = null;
            if (fss.Condition != null)
            {
                var condition = ExpressionHandle.GetRightAndMergePostCode(new(fss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));
                p.Block.Emit(new Code_Jump(out jumpOut, Code_Jump.OpCode.notEqual, condition, LVariableOrValue.ONE));
            }

            var forBlock = HandleBlock(p.With(fss.Statement));

            if (forBlock.Codes.Count > 0)
            {
                // for body
                p.Block.Emit(forBlock.Codes);


                // for Incrementors
                var incrementorsCodeStart = p.Block.Codes.Count;
                foreach (var inc in fss.Incrementors)
                {
                    ExpressionHandle.GetRight(new ExpressionHandle.Parameter(inc, p.Context, p.SemanticModel, p.Block, p.Block.Method));
                    p.Block.MergePostCodes();
                    // todo post++
                }

                if (incrementorsCodeStart == p.Block.Codes.Count)
                {
                    // empty Incrementors
                    var node = p.Block.Codes[conditionCodeStart];
                    forBlock.ContinueCall(node);
                    forBlock.NextCall(node);
                }
                else
                {
                    // not empty Incrementors
                    var node = p.Block.Codes[incrementorsCodeStart];
                    forBlock.ContinueCall(node);
                    forBlock.NextCall(node);
                }

                p.Block.ReturnCall += (node) => forBlock.ReturnCall(node);
            }
            if (jumpOut != null)
                p.Block.NextCall += (node) => jumpOut.JumpTo(node);

        }
    }

}
