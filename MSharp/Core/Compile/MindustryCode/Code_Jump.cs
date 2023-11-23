using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Utility;

namespace MSharp.Core.CodeAnalysis.Compile.MindustryCode
{
    internal class Code_Jump : BaseCode
    {
        public enum OpCode
        {
            /// <summary>
            /// =
            /// </summary>
            equal = 0,
            /// <summary>
            /// !=
            /// </summary>
            notEqual = 1,
            /// <summary>
            /// >=
            /// </summary>
            greaterThanEq = 2,
            /// <summary>
            /// <![CDATA[<]]>
            /// </summary>
            lessThan = 3,
            /// <summary>
            /// <![CDATA[<=]]>
            /// </summary>
            lessThanEq = 4,
            /// <summary>
            /// >
            /// </summary>
            greaterThan = 5,
            /// <summary>
            /// ALWAYS
            /// </summary>
            always = 6,
            // === not allowed
            strictEqual = 8,
        }

        public BaseCode? To;
        public readonly OpCode Op;
        public readonly LVariableOrValue? Left;
        public readonly LVariableOrValue? Right;

        public Code_Jump(out Code_Jump me, OpCode op, LVariableOrValue? left = null, LVariableOrValue? right = null)
        {
            Op = op;
            Left = left;
            Right = right;
            _variables.AddIfNotNull(left);
            _variables.AddIfNotNull(right);
            me = this;
        }

        public override string ToMindustryCodeString()
        {
            // jump 1 equal x false
            // jump 1 notEqual x false
            // jump 1 lessThan x false
            // jump 1 lessThanEq x false
            // jump 1 greaterThan x false
            // jump 1 greaterThanEq x false
            // jump 1 strictEqual x false
            // jump 1 always x false
            return $"jump {To?.Index ?? -1} {Op} {(Left != null ? Left.VariableOrValueString : "0")} {(Right != null ? Right.VariableOrValueString : "0")}";
        }

    }
}
