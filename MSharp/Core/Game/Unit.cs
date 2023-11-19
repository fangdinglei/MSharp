using MSharp.Core.Shared;
using System;
#pragma warning disable IDE1006,CS8618 // 命名样式
namespace MSharp.Core.Game
{
    /// <summary>
    /// <see href="https://www.mindustry-logic.xyz"/>
    /// </summary>
    [GameIgnore]
    public class GameObject
    {//TODO 检查类型
        /// <summary>
        /// 物品总数量
        /// </summary>
        [GameObjectData] public double @totalItems { get; }
        /// <summary>
        /// 第一个物品
        /// </summary>
        [GameObjectData] public object @firstItem { get; }
        /// <summary>
        /// 液体总数量
        /// </summary>
        [GameObjectData] public double @totalLiquids { get; }

        /// <summary>
        /// 总存储电量？？;
        /// </summary>
        [GameObjectData] public double @totalPower { get; }

        /// <summary>
        /// 单个物品容量 如核心只获取一个物体的容量
        /// </summary>
        [GameObjectData] public double @itemCapacity { get; }
        /// <summary>
        /// 单个液体容量
        /// <see cref="@itemCapacity">
        /// </summary>
        [GameObjectData] public double @liquidCapacity { get; }
        /// <summary>
        /// 电量容量
        /// </summary>
        [GameObjectData] public double @powerCapacity { get; }
        /// <summary>
        /// 电网电力存量
        /// </summary>
        [GameObjectData] public double @powerNetStored { get; }
        /// <summary>
        /// 电网容量
        /// </summary>
        [GameObjectData] public double @powerNetCapacity { get; }
        /// <summary>
        /// 电网输出
        /// </summary>
        [GameObjectData] public double powerNetIn { get; }
        /// <summary>
        /// 电网消耗
        /// </summary>
        [GameObjectData] public double powerNetOut { get; }

        /// <summary>
        /// 获取这个建筑物/单位内的子弹量
        /// </summary>
        [GameObjectData] public double @ammo { get; }
        /// <summary>
        /// 获取这个建筑物/单位内的子弹量上限
        /// </summary>
        [GameObjectData] public double @ammoCapacity { get; }
        /// <summary>
        /// 获取这个建筑物/单位的生命值
        /// </summary>
        [GameObjectData] public double @health { get; }
        /// <summary>
        ///  获取这个建筑物/单位的生命值上限
        /// </summary>
        [GameObjectData] public double @maxHealth { get; }
        /// <summary>
        /// 获取这个建筑物/单位的发热(主要是核反应堆)
        /// </summary>
        [GameObjectData] public double @heat { get; }
        /// <summary>
        /// 获取这个建筑物/单位的效率
        /// </summary>
        [GameObjectData] public double @efficiency { get; }
        /// <summary>
        /// 获取这个建筑物/单位的时间流速
        /// </summary>
        [GameObjectData] public double @timescale { get; }
        /// <summary>
        /// 获取这个炮塔/单位的旋转角度,建筑物则获取朝向(0为沿建筑物x轴方向, 逆时针)
        /// </summary>
        [GameObjectData] public double @rotation { get; }
        /// <summary>
        /// 获取这个建筑物/单位的x坐标
        /// </summary>
        [GameObjectData] public double @x { get; }
        /// <summary>
        /// 获取这个建筑物/单位的y坐标
        /// </summary>
        [GameObjectData] public double @y { get; }
        /// <summary>
        /// 获取这个建筑物/单位的射击x坐标
        /// </summary>
        [GameObjectData] public double @shootX { get; }
        /// <summary>
        /// 获取这个建筑物/单位的射击y坐标
        /// </summary>
        [GameObjectData] public double @shootY { get; }
        /// <summary>
        /// 获取这个建筑物/单位的大小(正方形边长大小)
        /// </summary>
        [GameObjectData] public double @size { get; }
        /// <summary>
        /// 获取这个建筑物/单位是否失效(被摧毁返回1 有效返回0)
        /// </summary>
        [GameObjectData] public double @dead { get; }
        /// <summary>
        /// 获取这个建筑物/单位的攻击范围
        /// </summary>
        [GameObjectData] public double @range { get; }
        /// <summary>
        /// 获取这个建筑物/单位的攻击状态(开火返回1 停火返回0)
        /// </summary>
        [GameObjectData] public double @shooting { get; }
        /// <summary>
        /// 获取这个建筑物/单位的阵营
        /// </summary>
        [GameObjectData] public double @team { get; }
        /// <summary>
        /// 返回这个建筑物/单位的类型
        /// </summary>
        [GameObjectData] public double @type { get; }
        /// <summary>
        ///  返回这个建筑物/单位的数字标记
        /// </summary>
        [GameObjectData] public double @flag { get; }
        /// <summary>
        /// 返回这个建筑物/单位是否被控制(处理器返回1 玩家返回2 编队返回3 如果都不是返回0)
        /// </summary>
        [GameObjectData] public double @controlled { get; }
        /// <summary>
        /// 不建议使用 将被移除 使用<see cref="@controlled">替代它
        /// </summary>
        [GameObjectData] public double @commanded { get; }
        /// <summary>
        /// 返回被标记单位控制者名字
        /// </summary>
        [GameObjectData] public double @name { get; }
        /// <summary>
        /// 获取单位的载荷数量
        /// </summary>
        [GameObjectData] public double @payloadCount { get; }
        /// <summary>
        /// 获取单位的载荷类型
        /// </summary>
        [GameObjectData] public double @payloadType { get; }

        ///// <summary>
        ///// 获取这个建筑物的配置(常用于分类器)
        ///// </summary>
        //[GameSensorField] public object @configure { get; set; }
    }
    [GameIgnore]
    public class Building : GameObject
    {
        /// <summary>
        /// 获取这个建筑物/单位的开启状态
        /// </summary>
        [GameObjectData] public bool @enabled { get; set; }

        /// <summary>
        /// 获取这个单位的配置(如工厂生产的物品)
        /// </summary>
        [GameObjectData] public double @config { get; set; }

        [GameApi("control enabled", 4, true)] public void SetEnabled(double enabled) { }
        [GameApi("control enabled", 4, true)] public void SetEnabled(bool enabled) { }

        [GameApi("control shoot", 4, true)] public void Shoot(double x, double y, double shoot) { }
        [GameApi("control shoot", 4, true)] public void Shoot(double x, double y, bool shoot) { }

        [GameApi("control shootp", 4, true)] public void ShootP(object target, double shoot) { }
        [GameApi("control shootp", 4, true)] public void ShootP(object target, bool shoot) { }

        [GameApi("control config", 4, true)] public void Config(object unknown) { }
    }
    [GameIgnore]
    public class Lamp : Building
    {
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="color"></param>
        //[Obsolete]
        //[GameApi("color")] public void Color(ColorData color) { }
    }
    public class Message : Building { }
    public class Display : Building { }

    [GameIgnore]
    public class Tower : Building
    {
        /// <summary>
        /// x , y, shoot
        /// </summary>
        [GameObjectData] public ValueTuple<int, int, bool> shoot { set; private get; }
        /// <summary>
        /// target shoot
        /// </summary>
        [GameObjectData] public ValueTuple<GameObject, bool> shootp { set; private get; }
    }
    [GameIgnore]
    public class Unit : GameObject
    {
        /// <summary>
        /// 返回一个单位的控制者(如果是处理器返回 processor 编队返回 leader 如果都不是返回 itself)
        /// </summary>
        [GameObjectData] public double @controller { get; }
        /// <summary>
        /// 获取这个单位的飞行状态
        /// </summary>
        [GameObjectData] public double @boosting { get; }
        /// <summary>
        /// 获取这个单位的挖矿x坐标
        /// </summary>
        [GameObjectData] public double @mineX { get; }
        /// <summary>
        /// 获取这个单位的挖矿y坐标
        /// </summary>
        [GameObjectData] public double @mineY { get; }
        /// <summary>
        ///  获取这个单位的挖矿状态
        /// </summary>
        [GameObjectData] public double @mining { get; }

        /// <summary>
        /// 原地不动，但继续采矿/建造
        /// </summary>
        [GameApi("ucontrol idle", 5, false)] public void Idle() { }
        /// <summary>
        /// 停止移动/建造/采矿
        /// </summary>
        [GameApi("ucontrol stop", 5, false)] public void Stop() { }
        /// <summary>
        /// 移动到
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [GameApi("ucontrol move", 5, false)] public void Move(double x, double y) { }
        /// <summary>
        /// 靠近
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        [GameApi("ucontrol approach", 5, false)] public void Approach(double x, double y, double radius) { }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <exception cref="NotImplementedException"></exception>
        [GameApi("ucontrol pathfind", 5, false)] public void PathFind(double x, double y) { throw new NotImplementedException(); }
        /// <summary>
        /// TODO
        /// </summary>
        [GameApi("ucontrol autoPathfind", 5, false)] public void AutoPathFind() { }
        /// <summary>
        /// 开始/结束 起飞
        /// </summary>
        [GameApi("ucontrol boost", 5, false)] public void Boost() { }
        /// <summary>
        /// 瞄准或开火
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="shoot">开火</param>
        [GameApi("ucontrol target", 5, false)] public void Target(double x, double y, bool shoot) { }
        /// <summary>
        /// 瞄准或开火（预判速度）
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="shoot">开火</param>
        [GameApi("ucontrol targetp", 5, false)] public void Targetp(Unit unit, bool shoot) { }
        /// <summary>
        /// 放下物资
        /// </summary>
        /// <param name="to"></param>
        /// <param name="mount"></param>
        [GameApi("ucontrol itemdrop", 5, false)] public void ItemDrop(Building to, double mount) { }
        /// <summary>
        /// 拿起物资
        /// </summary>
        /// <param name="from"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        [GameApi("ucontrol itemTake", 5, false)] public void ItemTake(Building from, string item, double amount) { }
        /// <summary>
        /// 放下载荷
        /// </summary>
        /// <param name="unit"></param>
        [GameApi("ucontrol itemDrop", 5, false)] public void PayDrop(Unit unit) { }
        /// <summary>
        /// 拿起载荷
        /// </summary>
        /// <param name="unit"></param>
        [GameApi("ucontrol payTake", 5, false)] public void PayTake(Unit unit) { }
        /// <summary>
        /// 进入或降落到单位下方的载荷方块中
        /// </summary>
        [GameApi("ucontrol payEnter", 5, false)] public void PayEnter() { }
        /// <summary>
        /// 采矿
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [GameApi("ucontrol mine", 5, false)] public void Mine(double x, double y) { }
        /// <summary>
        /// 标记
        /// </summary>
        /// <param name="value"></param>
        [GameApi("ucontrol flag", 5, false)] public void Flag(double value) { }
        /// <summary>
        /// 建造
        /// </summary>
        /// <param name="block"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="rotation"></param>
        /// <param name="config"></param>
        [GameApi("ucontrol build", 5, false)] public void Build(double block, double x, double y, double rotation, double config) { }
        /// <summary>
        /// 获取区块
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type">类型常量</param>
        /// <param name="building">建筑</param>
        /// <param name="floor">地面 矿或水等</param>
        [GameApi("ucontrol getBlock", 5, false)] public void GetBlock(double x, double y, out double type, out double building, out double floor) { throw new Exception(); }
        /// <summary>
        /// 是否靠近
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="result"></param>
        [GameApi("ucontrol within", 5, false)] public void Within(double x, double y, double radius, out bool result) { throw new Exception(); }
        /// <summary>
        /// 解除绑定
        /// </summary>
        [GameApi("ucontrol unbind", 5, false)] public void UnBind() { }
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
        //TODO 检查类型
        /// <summary>
        /// 自1970年1月1日到现在经过的毫秒
        /// </summary>
        public double @time;
        /// <summary>
        ///  指向当前对象自己
        /// </summary>
        public double @this;
        /// <summary>
        /// 获取当前对象自己的X坐标
        /// </summary>
        public double @thisx;
        /// <summary>
        ///  获取当前对象自己的Y坐标
        /// </summary>
        public double @thisy;
        /// <summary>
        /// 空气
        /// </summary>
        public double @air;
        /// <summary>
        /// 不可通过墙
        /// </summary>
        public double @soild;
        /// <summary>
        /// 绑定方块数
        /// </summary>
        public double @links;
        /// <summary>
        /// 逻辑执行行数
        /// </summary>
        public double @counter;
        /// <summary>
        /// 当前绑定单位
        /// </summary>
        public double @unit;
        /// <summary>
        /// 每tick执行行数
        /// </summary>
        public double @ipt;

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="color"></param>
        [Obsolete]
        public ColorData PackColor(double r, double g, double b, double a) { return null!; }

        [GameApi("ubind", -1, false)] public void UnitBind(UnitConst type) { }
        /// <summary>
        /// 处理器等待
        /// </summary>
        /// <param name="sec"></param>
        [GameApi("wait", -1, false)] public void Wait(double sec) { }
        /// <summary>
        /// 回到第一条
        /// </summary>
        [GameApi("end", -1, false)] public void End() { }
        /// <summary>
        /// 停止（没有发现和End区别）
        /// </summary>
        [GameApi("stop", -1, false)] public void Stop() { }

        // todo change to theirs class
        [GameApi("drawflush", -1, false)] public void DrawFlush(Display display) { }
        [GameApi("printflush", -1, false)] public void PrintFlush(Message message) { }


        [GameApi("draw clear", 6)] public void DrawClear(double r, double g, double b) { }
        [GameApi("draw color", 6)] public void DrawColor(double r, double g, double b, double a) { }
        [GameApi("draw col", 6)] public void DrawClear(ColorData color) { }
        [GameApi("draw stroke", 6)] public void DrawStroke(double width) { }
        [GameApi("draw line", 6)] public void DrawLine(double x, double y, double x2, double y2) { }
        [GameApi("draw rect", 6)] public void DrawRect(double x, double y, double width, double height) { }
        [GameApi("draw lineRect", 6)] public void DrawLineRect(double x, double y, double width, double height) { }
        [GameApi("draw poly", 6)] public void DrawPoly(double x, double y, double sides, double radius, double rotation) { }
        [GameApi("draw linePoly", 6)] public void DrawLinePoly(double x, double y, double sides, double radius, double rotation) { }
        [GameApi("draw triangle", 6)] public void DrawTriangle(double x, double y, double x2, double y2, double x3, double y3) { }
        [GameApi("draw image", 6)] public void DrawImage(double x, double y, GameConst image, double size, double rotation) { }
    }

    public class GameConst
    {
        public readonly string Name;

        public GameConst(string name)
        {
            Name = name;
        }
    }
    public class UnitConst : GameConst
    {
        static public UnitConst Mono = new("mono");

        public UnitConst(string v) : base(v) { }
    }

    public class ItemConst : GameConst
    {


        public ItemConst(string v) : base(v) { }
    }

    public class LiquidConst : GameConst
    {
        static public GameConst Water = new GameConst("water");


        public LiquidConst(string v) : base(v) { }
    }


    public class ColorData
    {

    }

}
#pragma warning restore IDE1006, CS8618 // 命名样式