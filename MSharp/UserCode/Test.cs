using MSharp.Core.Game;
using MSharp.Core.Logic;
using MSharp.Core.Shared;

namespace MSharp.UserCode
{
    public class TestCPU : GameCPU
    {
        static void B() { }
        public Memory cell1 = new Memory();

        public void A() { }

        [GameCall(MethodCallMode.UnsafeStacked)]
        public override void Main()
        {
            int a = 1;
            int b = a + 
                1;
            A();
            cell1.Write(1, 1);
        }
    }


}