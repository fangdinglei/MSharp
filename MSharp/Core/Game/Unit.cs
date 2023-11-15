using MSharp.Core.Shared;
using System;

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
        [GameSensorField] public double @totalItems;
        /// <summary>
        /// 第一个物品
        /// </summary>
        [GameSensorField] public object @firstItem;
        /// <summary>
        /// 液体总数量
        /// </summary>
        [GameSensorField] public double @totalLiquids;

        /// <summary>
        /// 总存储电量？？;
        /// </summary>
        [GameSensorField] public double @totalPower;

        /// <summary>
        /// 单个物品容量 如核心只获取一个物体的容量
        /// </summary>
        [GameSensorField] public double @itemCapacity;
        /// <summary>
        /// 单个液体容量
        /// <see cref="@itemCapacity">
        /// </summary>
        [GameSensorField] public double @liquidCapacity;
        /// <summary>
        /// 电量容量
        /// </summary>
        [GameSensorField] public double @powerCapacity;
        /// <summary>
        /// 电网电力存量
        /// </summary>
        [GameSensorField] public double @powerNetStored;
        /// <summary>
        /// 电网容量
        /// </summary>
        [GameSensorField] public double @powerNetCapacity;
        /// <summary>
        /// 电网输出
        /// </summary>
        [GameSensorField] public double powerNetIn;
        /// <summary>
        /// 电网消耗
        /// </summary>
        [GameSensorField] public double powerNetOut;

        /// <summary>
        /// 获取这个建筑物/单位内的子弹量
        /// </summary>
        [GameSensorField] public double @ammo;
        /// <summary>
        /// 获取这个建筑物/单位内的子弹量上限
        /// </summary>
        [GameSensorField] public double @ammoCapacity;
        /// <summary>
        /// 获取这个建筑物/单位的生命值
        /// </summary>
        [GameSensorField] public double @health;
        /// <summary>
        ///  获取这个建筑物/单位的生命值上限
        /// </summary>
        [GameSensorField] public double @maxHealth;
        /// <summary>
        /// 获取这个建筑物/单位的发热(主要是核反应堆)
        /// </summary>
        [GameSensorField] public double @heat;
        /// <summary>
        /// 获取这个建筑物/单位的效率
        /// </summary>
        [GameSensorField] public double @efficiency;
        /// <summary>
        /// 获取这个建筑物/单位的时间流速
        /// </summary>
        [GameSensorField] public double @timescale;
        /// <summary>
        /// 获取这个炮塔/单位的旋转角度,建筑物则获取朝向(0为沿建筑物x轴方向, 逆时针)
        /// </summary>
        [GameSensorField] public double @rotation;
        /// <summary>
        /// 获取这个建筑物/单位的x坐标
        /// </summary>
        [GameSensorField] public double @x;
        /// <summary>
        /// 获取这个建筑物/单位的y坐标
        /// </summary>
        [GameSensorField] public double @y;
        /// <summary>
        /// 获取这个建筑物/单位的射击x坐标
        /// </summary>
        [GameSensorField] public double @shootX;
        /// <summary>
        /// 获取这个建筑物/单位的射击y坐标
        /// </summary>
        [GameSensorField] public double @shootY;
        /// <summary>
        /// 获取这个建筑物/单位的大小(正方形边长大小)
        /// </summary>
        [GameSensorField] public double @size;
        /// <summary>
        /// 获取这个建筑物/单位是否失效(被摧毁返回1 有效返回0)
        /// </summary>
        [GameSensorField] public double @dead;
        /// <summary>
        /// 获取这个建筑物/单位的攻击范围
        /// </summary>
        [GameSensorField] public double @range;
        /// <summary>
        /// 获取这个建筑物/单位的攻击状态(开火返回1 停火返回0)
        /// </summary>
        [GameSensorField] public double @shooting;
        /// <summary>
        /// 获取这个单位的飞行状态
        /// </summary>
        [GameSensorField] public double @boosting;
        /// <summary>
        /// 获取这个单位的挖矿x坐标
        /// </summary>
        [GameSensorField] public double @mineX;
        /// <summary>
        /// 获取这个单位的挖矿y坐标
        /// </summary>
        [GameSensorField] public double @mineY;
        /// <summary>
        ///  获取这个单位的挖矿状态
        /// </summary>
        [GameSensorField] public double @mining;
        /// <summary>
        /// 获取这个建筑物/单位的阵营
        /// </summary>
        [GameSensorField] public double @team;
        /// <summary>
        /// 返回这个建筑物/单位的类型
        /// </summary>
        [GameSensorField] public double @type;
        /// <summary>
        ///  返回这个建筑物/单位的数字标记
        /// </summary>
        [GameSensorField] public double @flag;
        /// <summary>
        /// 返回这个建筑物/单位是否被控制(处理器返回1 玩家返回2 编队返回3 如果都不是返回0)
        /// </summary>
        [GameSensorField] public double @controlled;
        /// <summary>
        /// 返回一个单位的控制者(如果是处理器返回processor 编队返回 leader 如果都不是返回 itself)
        /// </summary>
        [GameSensorField] public double @controller;
        /// <summary>
        /// 不建议使用 将被移除 使用<see cref="@controlled">替代它
        /// </summary>
        [GameSensorField] public double @commanded;
        /// <summary>
        /// 返回被标记单位控制者名字
        /// </summary>
        [GameSensorField] public double @name;
        /// <summary>
        /// 获取这个单位的配置(如工厂生产的物品)
        /// </summary>
        [GameSensorField] public double @config;
        /// <summary>
        /// 获取单位的载荷数量
        /// </summary>
        [GameSensorField] public double @payloadCount;
        /// <summary>
        /// 获取单位的载荷类型
        /// </summary>
        [GameSensorField] public double @payloadType;
        /// <summary>
        /// 获取这个建筑物/单位的开启状态
        /// </summary>
        [GameSensorField] public double @enabled;
        /// <summary>
        /// 获取这个建筑物的配置(常用于分类器)
        /// </summary>
        [GameSensorField] public object @configure;
    }
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
