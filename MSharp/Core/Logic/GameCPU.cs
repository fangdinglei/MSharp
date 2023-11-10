using MSharp.Core.Game;
using MSharp.Core.Shared;

namespace MSharp.Core.Logic
{
    /// <summary>
    /// CPU逻辑
    /// </summary>
    public abstract class GameCPU
    {
        Processor self = new Processor();
        [GameCall(MethodCallMode.Inline)]
        public abstract void Main();
    }


}
