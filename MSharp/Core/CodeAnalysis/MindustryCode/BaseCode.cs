using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.MindustryCode
{
    internal interface IMindustryCodeContainer
    {
        void AddCall();
        void Merge(IMindustryCodeContainer other);
    }

    internal class MindustryCodeContainer
    {
        public readonly List<BaseCode> Codes = new();

        public void AddCall()
        {

        }

    }
    internal abstract class BaseCode
    {
        protected List<LVariableOrValue> _variables = new List<LVariableOrValue>();
        public IReadOnlyList<LVariableOrValue> Variables => _variables;


        public BaseCode DeepClone()
        {
            throw new System.NotImplementedException();
        }

        public abstract string ToMindustryCodeString();
    }
    /// <summary>
    /// 命名与像素工厂代码命名一致 方便转换 特别的相反的两个逻辑判断(<![CDATA[< <= > >= = !=]]>)枚举的值 xor后结果为1
    /// 
    /// </summary>
    internal enum MindustryOperatorKind
    {
        /// <summary>
        /// 加法 
        /// </summary>
        add = 1,
        /// <summary>
        /// 减法
        /// </summary>
        sub = 2,
        /// <summary>
        /// 乘法
        /// </summary>
        mul = 3,
        /// <summary>
        /// 除法
        /// </summary>
        dic = 4,
        /// <summary>
        /// 整除 like (int)(op1/op2)
        /// </summary>
        idiv = 5,
        /// <summary>
        /// 余数
        /// </summary>
        mod = 6,
        /// <summary>
        /// 幂数  like op1^op2
        /// </summary>
        pow = 7,
        /// <summary>
        /// =
        /// </summary>
        equal = 8,
        E = 8,
        /// <summary>
        /// !=
        /// </summary>
        notEqual = 9,
        NE = 9,
        /// <summary>
        /// &&
        /// </summary>
        land = 10,
        AND = 10,


        lessThanEq = 12,
        LE = 12,

        greaterThan = 13,
        GT = 13,

        greaterThanEq = 14,
        GE = 14,

        /// <summary>
        /// <![CDATA[<]]>
        /// </summary>
        lessThan = 15,
        LT = 16,

        /// <summary>
        /// like js ===
        /// <br/> 0 EQUAL null=>true   0 strictEqual null=>false
        /// </summary>
        strictEqual = 15,
        /// <summary>
        /// 左移
        /// </summary>
        shl = 16,
        /// <summary>
        /// 
        /// </summary>
        shr = 17,

        /// <summary>
        /// |
        /// </summary>
        or = 18,
        /// <summary>
        /// &
        /// </summary>
        and = 19,
        /// <summary>
        /// 
        /// </summary>
        xor = 20,
        /// <summary>
        /// 按位取反 ~
        /// </summary>
        not = 21,

        max = 22,
        min = 23,
        /// <summary>
        /// I don't know
        /// </summary>
        angle = 24,
        /// <summary>
        /// like sqrt(op1^2+op1^2)
        /// </summary>
        len = 25,
        /// <summary>
        /// I don't know
        /// </summary>
        noise = 26,

        abs = 27,
        /// <summary>
        /// log 2 op1
        /// </summary>
        log = 28,
        /// <summary>
        /// log 10 op1
        /// </summary>
        log10 = 29,
        sin = 30,
        cos = 31,
        tan = 32,
        floor = 33,
        ceil = 34,
        sqrt = 35,
        /// <summary>
        /// random [0,1)
        /// </summary>
        rand = 36,

    }
    internal class Code_Operation : BaseCode
    {
        public readonly MindustryOperatorKind Operation;
        public readonly LVariableOrValue Left;
        public readonly LVariableOrValue? Right;
        public readonly LVariableOrValue Result;
        public Code_Operation(MindustryOperatorKind op, LVariableOrValue result, LVariableOrValue op1, LVariableOrValue op2)
        {
            Operation = op;
            Left = op1;
            Right = op2;
            Result = result;
            _variables.Add(op1);
            _variables.Add(op2);
        }

        public override string ToMindustryCodeString()
        {
            // sample  op mul val num1 num2
            var r = Right == null ? string.Empty : " " + Right!.VariableOrValueString;
            return $"op {Operation} {Result!.VariableOrValueString} {Left!.VariableOrValueString}{r}";
        }
    }
    internal class Code_Assign : BaseCode
    {
        public readonly LVariableOrValue Left;
        public readonly LVariableOrValue Right;
        public Code_Assign(LVariableOrValue left, LVariableOrValue right)
        {
            Left = left;
            Right = right;
            _variables.Add(left);
            _variables.Add(right);
        }

        public override string ToMindustryCodeString()
        {
            // sample  set a 2    set a b
            return $"set {Left.VariableOrValueString} {Right.VariableOrValueString}";
        }
    }
    internal class Code_Sensor : BaseCode
    {
        public readonly LVariableOrValue Result;
        public readonly LVariableOrValue Object;
        public readonly string Member;
        public Code_Sensor(LVariableOrValue result, LVariableOrValue @object, string member)
        {
            Debug.Assert(member.StartsWith('@'), "sensor member should start with @");
            Result = result;
            Object = @object;
            Member = member;
            _variables.Add(result);
            _variables.Add(@object);
        }

        public override string ToMindustryCodeString()
        {
            // sample  sensor result block1 @copper
            return $"sensor {Result.VariableOrValueString} {Object.VariableOrValueString} {Member}";
        }
    }

}
