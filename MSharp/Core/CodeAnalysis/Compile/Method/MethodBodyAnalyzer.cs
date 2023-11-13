using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using MSharp.Core.CodeAnalysis.Compile.Method.StatementHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSharp.Core.CodeAnalysis.Compile.Method
{
    internal class MethodBodyAnalyzer : BaseAnalyzer
    {
        Dictionary<Type, StatementHandle> _handles = new();

        public MethodBodyAnalyzer()
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
                .Select(it => (StatementHandle?)Activator.CreateInstance(it))
                .ToList().ForEach(it =>
                {
                    Debug.Assert(it != null);
                    foreach (Type type in it.Types)
                    {
                        _handles.Add(type, it);
                    }
                });
        }

        public LBlock Analyze(CompileContext context, LMethod method, SemanticModel semanticModel, List<StatementSyntax> syntaxes)
        {
            var block = new LBlock(method);
            method.Block=block;
            foreach (var statement in syntaxes)
            {
                Type? type = null;
                if (statement is ExpressionStatementSyntax ess)
                    type = ess.Expression.GetType();
                else
                    type = statement.GetType();
                if (!_handles.TryGetValue(type, out var handle))
                {
                    Console.WriteLine("语义分析：未知的语句类型" + type);
                    continue;
                }
                handle.Handle(new StatementHandleParameters(context, this, semanticModel, block, statement));
            }
            return block;
        }

    }
}