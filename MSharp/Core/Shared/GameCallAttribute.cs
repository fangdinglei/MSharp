using System;

namespace MSharp.Core.Shared
{
    /// <summary>
    /// 标记调用的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class GameCallAttribute : Attribute
    {
        public MethodCallMode Mode;

        public GameCallAttribute(MethodCallMode mode)
        {
            Mode = mode;
        }
    }

}
