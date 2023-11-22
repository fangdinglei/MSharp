using Microsoft.CodeAnalysis;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Language
{
    /// <summary>
    /// 变量 一个同样的变量应该具有同样的引用
    /// </summary>
    internal class LVariable
    {
        public const string TEMP_PREFIX = "_t_var_";


        public enum VariableType
        {
            Temp,
            LocalVar,
            Field,
        }

        public LClass? Class;
        public LMethod? Method;
        public int? Index;
        public string? Name;
        public ITypeSymbol? Type;
        public ISymbol? Symbol;

        public readonly VariableType Kind;

        public string RealName => (Index.HasValue ? TEMP_PREFIX + Index : Name)!;

        public LVariable(LClass clazz, ITypeSymbol type, ISymbol symbol, string name)
        {
            Debug.Assert(clazz != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Class = clazz;
            Kind = VariableType.Field;
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public LVariable(LMethod method, string name, ITypeSymbol type, ISymbol symbol)
        {
            Debug.Assert(method != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Kind = VariableType.LocalVar;
            Method = method;
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public LVariable(LMethod method, int index, ITypeSymbol type)
        {
            Kind = VariableType.Temp;
            Method = method;
            Index = index;
            Type = type;
        }

        public LVariable(LMethod method, string name, int index, ITypeSymbol? type, ISymbol? symbol, VariableType kind)
        {
            Debug.Assert(method != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Method = method;
            Index = index;
            Name = name;
            Type = type;
            Symbol = symbol;
            Kind = kind;
        }


        public override string ToString()
        {
            return Type + " " + RealName;
        }
    }
}
