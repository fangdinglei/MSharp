using MSharp.Core.Shared;
using System;

namespace MSharp.Core.Game
{
    [GameIgnore]
    public class GameObject { }
    [GameIgnore]
    public class Building : GameObject
    {

    }
    [GameIgnore]
    public class Unit : GameObject
    {
        /// <summary>
        /// 原地不动，但继续采矿/建造
        /// </summary>
        public void Idea() { }
        /// <summary>
        /// 停止移动/建造/采矿
        /// </summary>
        public void Stop() { }
        /// <summary>
        /// 移动到
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Move(double x, double y) { }
        /// <summary>
        /// 靠近
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        public void Approach(double x, double y, double radius) { }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void PathFind(double x, double y) { throw new NotImplementedException(); }
        /// <summary>
        /// TODO
        /// </summary>
        public void AutoPathFind() { }
        /// <summary>
        /// 开始/结束 起飞
        /// </summary>
        public void Boost() { }
        /// <summary>
        /// 瞄准或开火
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="shoot">开火</param>
        public void Target(double x, double y, bool shoot) { }
        /// <summary>
        /// 瞄准或开火（预判速度）
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="shoot">开火</param>
        public void Targetp(Unit unit, bool shoot) { }
        /// <summary>
        /// 放下物资
        /// </summary>
        /// <param name="to"></param>
        /// <param name="mount"></param>
        public void ItemDrop(Building to, double mount) { }
        /// <summary>
        /// 拿起物资
        /// </summary>
        /// <param name="from"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        public void ItemTake(Building from, string item, double amount) { }
        /// <summary>
        /// 放下载荷
        /// </summary>
        /// <param name="unit"></param>
        public void PayDrop(Unit unit) { }
        /// <summary>
        /// 拿起载荷
        /// </summary>
        /// <param name="unit"></param>
        public void PayTake(Unit unit) { }
        /// <summary>
        /// 进入或降落到单位下方的载荷方块中
        /// </summary>
        public void PayEnter() { }
        /// <summary>
        /// 采矿
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Mine(double x, double y) { }
        /// <summary>
        /// 标记
        /// </summary>
        /// <param name="value"></param>
        public void Flag(double value) { }
        /// <summary>
        /// 建造
        /// </summary>
        /// <param name="block"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotation"></param>
        /// <param name="config"></param>
        public void Build(double block, double x, double y, double rotation, double config) { }
        /// <summary>
        /// 获取区块
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type">类型常量</param>
        /// <param name="building">建筑</param>
        /// <param name="floor">地面 矿或水等</param>
        public void GetBlock(double x, double y, out double type, out double building, out double floor) { throw new Exception(); }
        /// <summary>
        /// 是否靠近
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="result"></param>
        public void Within(double x, double y, double radius, out bool result) { throw new Exception(); }
        /// <summary>
        /// 解除绑定
        /// </summary>
        public void Bind() { }
    }
    [GameIgnore]
    public class Memory : GameObject
    {
        public double Read(double at) { return 0; }

        public void Write(double value, double at) { }
    }
    [GameIgnore]
    public class Processor : Building
    {

        static public void UnitBind(string type) { }
        /// <summary>
        /// 处理器等待
        /// </summary>
        /// <param name="sec"></param>
        static public void Wait(double sec) { }
        /// <summary>
        /// 回到第一条
        /// </summary>
        static public void End() { }
        /// <summary>
        /// 停止（没有发现和End区别）
        /// </summary>
        static public void Stop() { }
    }


}
