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
        public Unit t = new Unit();
        public GameObject g = new Tower();
        public void A() { }

        [CustomerCall(MethodCallMode.UnsafeStacked)]
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
            //float b = 2;
            //float c = ++b;
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
            //@unit.Idle();
            //
            //@unit.Target(1, 2, true);
            //unit.Boost(out var se);
            //self.UnitBind(UnitConst.Mono);

            //for (int i = 0; i < 10; i++)
            //{
            //    i++;
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    i++;
            //}
            //while (true)
            //{
            //    int i = 1;
            //    i++;
            //}
            int i = 0;
            if (1 == 2)
            {
                i = 2;
            }
            else if (2 == 3)
            {
                i = 3;
            }
            else
            {
                i = 5;
            }

            // TODO内联函数调用
            // TODO函数参数类型确定 含是否 out
            // TODO编译优化
            // 中间变量优化 =》
            //      如果几个语句之间没有跳入跳出，没有游戏调用，则其是可优化的
            //      在可优化域中，如果有   a=xxx op xxx, b = a ，则其是可优化的 优化为 b = xxx op xxx
            //      在可优化域中，如果有   a=xxx logic op xxx, jxx a != 1,可优化为 jxx xxx （反 logic op） xxx
            //      在可优化域中，如果有 赋值操作是无效的，则将其删除
            // LVariableOrValue 多参数改 list


        }
    }


}