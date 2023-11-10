using System.IO;
using System.Text.RegularExpressions;

namespace MSharp.Core.Utility
{
    public class CommonUtility
    {
        /// <summary>
        /// debug模式下获取代码路径
        /// </summary>
        /// <returns></returns>
        static public string GetCodePathWhenDebug()
        {
            var path = Directory.GetCurrentDirectory();
            return Regex.Match(path, @".*(?=bin\\Debug)").Value;
        }
    }
}
