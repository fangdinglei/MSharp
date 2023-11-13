using System;

namespace MSharp.Core.Shared
{
    /// <summary>
    /// 标记只是用于代码提示 而不参与分析的代码
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class, Inherited = false)]
    public class GameIgnoreAttribute : Attribute
    {

    }

}
