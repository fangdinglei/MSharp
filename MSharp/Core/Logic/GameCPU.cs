using MSharp.Core.Game;
using MSharp.Core.Shared;

namespace MSharp.Core.Logic
{
    /// <summary>
    /// CPU逻辑
    /// </summary>
    [GameIgnore]
    public abstract class GameCPU
    {
        protected Processor self = new Processor();
        protected Unit @unit = null!;

        [CustomerCall(MethodCallMode.Inline)]
        public abstract void Main();
    }


}
