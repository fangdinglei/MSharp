using Microsoft.CodeAnalysis;

namespace MSharp.Core.Compile
{
    public class BaseAnalyzer
    {
        /// <summary>
        /// 获取名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected string GetName(ITypeSymbol symbol)
        {
            return TypeUtility.GetName(symbol);
        }
        /// <summary>
        /// 获取全名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected string GetFullName(ITypeSymbol symbol)
        {
            return symbol.ToString()!;
        }
    }
}
