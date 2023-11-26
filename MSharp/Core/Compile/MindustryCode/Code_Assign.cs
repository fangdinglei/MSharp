using MSharp.Core.Compile.Language;

namespace MSharp.Core.Compile.MindustryCode
{
    internal class Code_Assign : BaseCode
    {
        public readonly LVariableOrValue Left;
        public readonly LVariableOrValue Right;

        public Code_Assign(LVariable left, LVariableOrValue right)
        {
            Left = new LVariableOrValue(left);
            Right = right;
            W(Left);
            R(Right);
        }

        public override string ToMindustryCodeString()
        {
            // sample  set a 2    set a b
            return $"set {Left.VariableOrValueString} {Right.VariableOrValueString}";
        }
    }
}
