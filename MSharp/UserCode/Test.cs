using MSharp.Core.Game;
using MSharp.Core.Logic;
using MSharp.Core.Shared;

namespace MSharp.UserCode
{
    public class B
    {
        public void A(float a)
        {

        }
    }
    public class TestCPU : GameCPU
    {
        B b = new B();
        public Memory cell1 = new Memory();

        public void A() { }

        [GameCall(MethodCallMode.UnsafeStacked)]
        public override void Main()
        {
            float a = -1 + (+2.1f);
            float b = a + a;
            //b = a;
            //int a = 1;
            //int b = a + 
            //    1;
            //A();
            //cell1.Write(1, 1);
        }
    }


}