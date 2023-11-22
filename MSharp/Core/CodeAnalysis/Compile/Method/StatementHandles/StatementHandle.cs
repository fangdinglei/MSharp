using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles;
using MSharp.Core.CodeAnalysis.MindustryCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSharp.Core.CodeAnalysis.Compile.Method.StatementHandles
{
    internal class StatementHandleParameters
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
    }

    internal class StatementManager
    {
        Dictionary<Type, StatementHandle> _handles = new();

        public StatementManager()
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
                    res.StatementManager = this;
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

        public void Handle(StatementSyntax statement, CompileContext context, SemanticModel semanticModel, LBlock block)
        {
            Type? type = null;
            type = statement.GetType();
            if (!_handles.TryGetValue(type, out var handle))
            {
                Console.WriteLine("语义分析：未知的语句类型\n" + statement);
                return;
            }
            handle.Handle(new StatementHandleParameters(context, semanticModel, block, statement));
            block.MergePostCodes();
        }
    }

    internal abstract class StatementHandle
    {
        public abstract List<Type> Types { get; }

        public StatementManager StatementManager = null!;

        public abstract void Handle(StatementHandleParameters parameters);

        protected StatementSyntax[] GetStatements(StatementSyntax ss)
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
    }

    internal class LocalVariableStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() { typeof(LocalDeclarationStatementSyntax) };

        public override void Handle(StatementHandleParameters p)
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

        public override void Handle(StatementHandleParameters p)
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

        public override void Handle(StatementHandleParameters p)
        {
            IfStatementSyntax iss = (IfStatementSyntax)p.Syntax;
            var condition = ExpressionHandle.GetRight(new(iss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));
            p.Block.MergePostCodes();
            var body = iss.Statement;

            var ifBlock = new LBlock(p.Block.Method);
            StatementSyntax[] statements = GetStatements(body);

            foreach (var item in statements)
                p.Context.StatementManager.Handle(item, p.Context, p.SemanticModel, ifBlock);

            // jump to else/else if/next
            p.Block.Emit(new Code_Jump(out var jumpNext, Code_Jump.OpCode.notEqual, condition, new LVariableOrValue(1)));
            // if body
            p.Block.Emit(ifBlock.Codes);

            p.Block.ReturnCall += (node) => ifBlock.ReturnCall(node);
            p.Block.ContinueCall += (node) => ifBlock.ContinueCall(node);

            if (iss.Else != null)
            {
                // else/else if
                var elseStm = iss.Else.Statement;
                var elseBlock = new LBlock(p.Block.Method);
                if (elseStm is IfStatementSyntax)
                {
                    Handle(new StatementHandleParameters(p.Context, p.SemanticModel, elseBlock, elseStm));
                }
                else
                {
                    var elseStms = GetStatements(elseStm);
                    foreach (var item in elseStms)
                        p.Context.StatementManager.Handle(item, p.Context, p.SemanticModel, elseBlock);
                }

                if (elseBlock.Codes.Count() != 0)
                {
                    // end of if : jump out
                    p.Block.Emit(new Code_Jump(out var jumpOut, Code_Jump.OpCode.always));
                    // jump here when if is not true
                    jumpNext.To = elseBlock.Codes[0];
                    // else if body
                    p.Block.Emit(elseBlock.Codes);

                    p.Block.ReturnCall += (node) => elseBlock.ReturnCall(node);
                    p.Block.ContinueCall += (node) => elseBlock.ContinueCall(node);
                    p.Block.NextCall += (node) => elseBlock.NextCall(node);
                    // end of if : jump out
                    p.Block.NextCall += (node) => jumpOut.To = node;
                }

            }
            // if no else of else is empty,set jump next
            if (jumpNext.To == null)
            {
                p.Block.NextCall += (node) => jumpNext.To = node;
            }

        }
    }

    internal class WhileStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(WhileStatementSyntax) ,
        };

        public override void Handle(StatementHandleParameters p)
        {
            WhileStatementSyntax wss = (WhileStatementSyntax)p.Syntax;

            var whileBlock = new LBlock(p.Block.Method);
            StatementSyntax[] statements = GetStatements(wss.Statement);


            var conditionCodeStart = p.Block.Codes.Count;
            var condition = ExpressionHandle.GetRight(new(wss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));
            p.Block.MergePostCodes();

            foreach (var item in statements)
                p.Context.StatementManager.Handle(item, p.Context, p.SemanticModel, whileBlock);

            if (whileBlock.Codes.Count > 0)
            {
                // jump out of while
                p.Block.Emit(new Code_Jump(out var jumpOut, Code_Jump.OpCode.notEqual, condition, new LVariableOrValue(1)));
                // while body
                p.Block.Emit(whileBlock.Codes);
                // jump begin of while
                p.Block.Emit(new Code_Jump(out var jumpBegin, Code_Jump.OpCode.always));
                // continue in block  or  end of while should jump to begin
                jumpBegin.To = p.Block.Codes[conditionCodeStart];
                whileBlock.ContinueCall(jumpBegin.To);
                // next of while block is 
                whileBlock.NextCall(jumpBegin);
                p.Block.ReturnCall += (node) => whileBlock.ReturnCall(node);

                // register jump out
                if (jumpOut != null)
                    p.Block.NextCall += (node) => jumpOut.To = node;
            }
        }
    }

    internal class ForStatementHandle : StatementHandle
    {
        public override List<Type> Types => new List<Type>() {
            typeof(ForStatementSyntax) ,
        };

        public override void Handle(StatementHandleParameters p)
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
                var condition = ExpressionHandle.GetRight(new(fss.Condition, p.Context, p.SemanticModel, p.Block, p.Block.Method));
                p.Block.MergePostCodes();
                p.Block.Emit(new Code_Jump(out jumpOut, Code_Jump.OpCode.notEqual, condition, LVariableOrValue.ONE));
            }

            var forBlock = new LBlock(p.Block.Method);
            StatementSyntax[] statements = GetStatements(fss.Statement);

            foreach (var item in statements)
                p.Context.StatementManager.Handle(item, p.Context, p.SemanticModel, forBlock);

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
                p.Block.NextCall += (node) => jumpOut.To = node;

        }
    }

}
