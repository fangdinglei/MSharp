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
        public Tower tower = new Tower();
        public GameObject g = new Tower();
        public void A() { }

        [GameCall(MethodCallMode.UnsafeStacked)]
        public override void Main()
        {
            //float a = -1 + (+2.1f);
            //float b = a + a;
            //b = a;
            //double c;
            //if ((dynamic)1)
            //    tower.shoot = (1, 2, true);
            //else if (1==2)
            //{
            //    tower.shoot = (1, 2, true);
            //}
            float b = 2;
            b = ++b;
            //while (b==3)
            //{
            //    tower.shoot = (1, 2, true);
            //}
            //for (int i = 0,c=2; i < 10; i++,c++)
            //{

            //}

            //b = a;
            //int a = 1;
            //int b = a + 
            //    1;
            //A();
            //tower.Write(1, 1);
        }
    }


}