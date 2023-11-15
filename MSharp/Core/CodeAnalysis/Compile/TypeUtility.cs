using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace MSharp.Core.CodeAnalysis.Compile
{
    /// <summary>
    /// Part of the code in this project is used both at runtime (Type) and as a reference (Symbol) during semantic analysis. In these two scenarios, use full names to compare whether they are of the same type.
    /// <br/>本项目的部分代码既在运行时被使用（Type），也在运行语义分析时被作为参考（Symbol）。在这两个场景下，使用全名比较他们是否是同一个类型。
    /// </summary>
    internal class TypeUtility
    {
        /// <summary>
        /// determine if the symbol is a specific type
        /// <br/>判断一个 symbol 是否是 type 的子类
        /// 
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsSonOf(ITypeSymbol? symbol, Type type)
        {
            while (symbol != null)
            {
                if (GetFullName(symbol) == type.FullName)
                    return true;
                symbol = symbol.BaseType;
            }
            return false;
        }

        /// <summary>
        /// 获取名称
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public string GetName(ITypeSymbol symbol)
        {
            return symbol.Name;
        }
        /// <summary>
        /// 获取全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public string GetFullName(ITypeSymbol symbol)
        {
            return symbol.ToString()!;
        }

        public bool HasAttribute(ISymbol? symbol, Type type)
        {
            if (symbol == null)
                return false;
            return null != symbol!.GetAttributes().Where(it => GetFullName(it!.AttributeClass!) == type.FullName).FirstOrDefault();
        }

    }
}
