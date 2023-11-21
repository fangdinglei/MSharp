using Microsoft.CodeAnalysis;

namespace MSharp.Core.CodeAnalysis.Compile
{
    public class BaseAnalyzer
    {
        TypeUtility _typeUtility = new TypeUtility();
        /// <summary>
        /// 获取名
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        protected string GetName(ITypeSymbol symbol)
        {
            return _typeUtility.GetName(symbol);
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
