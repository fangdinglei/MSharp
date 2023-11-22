using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace MSharp.Core.CodeAnalysis.Language
{
    internal class FieldTable
    {
        Dictionary<string, LVariable> defines = new();
        Dictionary<ISymbol, LVariable> SymbolDict = new(SymbolEqualityComparer.Default);
        LClass _class;

        public FieldTable(LClass @class)
        {
            _class = @class;
        }

        public LVariable Add(ISymbol symbol, ITypeSymbol type, string name)
        {
            var v = new LVariable(_class, type, symbol, name);
            SymbolDict[symbol] = v;
            defines[name] = v;
            return v;
        }
        public bool TryGet(ISymbol symbol, out LVariable? variable)
        {
            return SymbolDict.TryGetValue(symbol, out variable);
        }

    }
}
