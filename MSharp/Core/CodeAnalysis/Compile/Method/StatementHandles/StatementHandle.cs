using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles;
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
                Console.WriteLine("语义分析：未知的语句类型" + type);
                return;
            }
            handle.Handle(new StatementHandleParameters(context, semanticModel, block, statement));
        }
    }

    internal abstract class StatementHandle
    {
        public abstract List<Type> Types { get; }

        public StatementManager StatementManager = null!;

        public abstract void Handle(StatementHandleParameters parameters);
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
                var var2 = p.Block.Method.VariableTable.Add(typeInfo.Type!, symbol!, variable.Identifier.ToString());
                if (variable.Initializer != null)
                {
                    ExpressionHandle.Assign(var2, variable.Initializer!.Value, p.Context, p.SemanticModel, p.Block.Method);
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
            ExpressionHandle.GetValue(ess.Expression, p.Context, p.SemanticModel, p.Block.Method);
        }
    }

}
