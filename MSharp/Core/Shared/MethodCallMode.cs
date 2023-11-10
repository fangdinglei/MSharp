namespace MSharp.Core.Shared
{
    /// <summary>
    /// 方法类型
    /// </summary>
    public enum MethodCallMode
    {
        Default = 0,
        /// <summary>
        /// 内联
        /// </summary>
        Inline = 0,
        /// <summary>
        /// 基于堆栈的
        /// </summary>
        Stacked = 1,
        /// <summary>
        /// 不进行保护现场的调用，可能造成同名变量覆盖，慎用
        /// </summary>
        UnsafeStacked = 2,

    }

}
