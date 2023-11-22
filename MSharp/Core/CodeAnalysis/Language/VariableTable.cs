using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Language
{
    internal class VariableTable
    {
        int ptr = 0;
        Dictionary<string, LVariable> defines = new();
        Dictionary<ISymbol, LVariable> SymbolDict = new(SymbolEqualityComparer.Default);
        LMethod _method;

        public VariableTable(LMethod method)
        {
            _method = method;
        }

        public LVariable AddLocalVariable(ITypeSymbol type, ISymbol symbol, string name)
        {
            string suffix = "";
            int ptr = 0;
            while (true)
            {
                // process same name
                if (defines.ContainsKey(name + suffix))
                {
                    ptr++;
                    suffix = "_" + ptr;
                    continue;
                }
                var v = new LVariable(_method, name + suffix, type, symbol);
                defines.Add(v.RealName, v);
                SymbolDict.Add(symbol, v);
                return v;
            }
        }

        public LVariable AddTempVariable(ITypeSymbol type)
        {
            while (true)
            {
                var v = new LVariable(_method, ++ptr, type);
                if (defines.ContainsKey(v.RealName))
                    continue;
                defines.Add(v.RealName, v);
                return v;
            }
        }

        public LVariable ReNameVariable(LVariable variable)
        {
            // 重命名
            if (variable.Kind != LVariable.VariableType.Temp)
            {
                Debug.Assert(variable.Name != null);
                // 如果没有，直接使用原名
                if (!defines.ContainsKey(variable.RealName))
                {
                    defines.Add(variable.RealName, variable);
                    return variable;
                }
                // 使用自动生成的名称
                while (true)
                {
                    var v = new LVariable(_method, variable.Name, ++ptr, variable.Type, variable.Symbol, variable.Kind);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    return v;
                }
            }
            else
            {
                // 使用自动生成的名称
                while (true)
                {
                    var v = new LVariable(_method, ++ptr, variable.Type!);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    SymbolDict.Add(v.Symbol!, v);
                    return v;
                }
            }
            throw new Exception();
        }

        public LVariable Get(ISymbol symbol)
        {
            if (TryGet(symbol, out var v))
                return v!;
            throw new Exception("variable not defined");
        }
        public bool TryGet(ISymbol symbol, out LVariable? variable)
        {
            return SymbolDict.TryGetValue(symbol, out variable);
        }
        public ICollection<LVariable> GetAll()
        {
            return defines.Values;
        }

    }
}
