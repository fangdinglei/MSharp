using Microsoft.CodeAnalysis;

namespace MSharp.Core.CodeAnalysis.Compile
{
    public class BaseAnalyzer
    {

        /// <summary>
        /// 获取名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected string GetName(INamespaceOrTypeSymbol symbol)
        {
            return symbol.Name;
        }
        /// <summary>
        /// 获取全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected string GetFullName(INamespaceOrTypeSymbol symbol)
        {
#pragma warning disable CS8603 // 可能返回 null 引用。
            return symbol.ToString();
#pragma warning restore CS8603 // 可能返回 null 引用。
            //var res = GetName(symbol);
            //INamespaceOrTypeSymbol? node = symbol.ContainingNamespace as INamespaceOrTypeSymbol;
            //while (node != null)
            //{
            //    res = GetName(node) + "." + res;
            //    node = node.ContainingSymbol as INamespaceOrTypeSymbol;
            //    if (node == null || (node is INamespaceSymbol ns && ns.IsGlobalNamespace))
            //        break;
            //}
            //return res;
        }
    }
}
