using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MSharp.Core.Compile.Language
{
    internal class LClass
    {
        private Dictionary<IMethodSymbol, LMethod> _functions = new(SymbolEqualityComparer.Default);

        public readonly CompileContext Context;
        /// <summary>
        /// 类的定义，暂时没用，先记着
        /// </summary>
        public readonly INamedTypeSymbol Symbol;
        /// <summary>
        /// 字段表（不管是属性还是字段都记这）
        /// </summary>
        public readonly FieldTable VariableTable;

        /// <summary>
        /// 类中的函数
        /// </summary>
        public IReadOnlyDictionary<IMethodSymbol, LMethod> Functions => _functions;

        public LClass(CompileContext compileContext, INamedTypeSymbol symbol)
        {
            Context = compileContext;
            Symbol = symbol;
            VariableTable = new FieldTable(this);
        }

        public LMethod CreateMethod(IMethodSymbol symbol)
        {
            var res = new LMethod(this, symbol);
            _functions.Add(symbol, res);
            Context.AddMethod(symbol, res);
            return res;
        }
    }
}
