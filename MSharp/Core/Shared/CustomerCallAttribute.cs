using System;

namespace MSharp.Core.Shared
{
    /// <summary>
    /// 标记调用的类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CustomerCallAttribute : Attribute
    {
        public MethodCallMode Mode;

        public CustomerCallAttribute(MethodCallMode mode)
        {
            Mode = mode;
        }
    }

}
