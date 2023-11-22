using MSharp.Core.CodeAnalysis.Language;

namespace MSharp.Core.CodeAnalysis.MindustryCode
{
    internal class Code_Assign : BaseCode
    {
        public readonly LVariableOrValue Left;
        public readonly LVariableOrValue Right;
        public Code_Assign(LVariable left, LVariableOrValue right)
        {
            Left = new LVariableOrValue(left);
            Right = right;
            _variables.Add(Left);
            _variables.Add(right);
        }

        public override string ToMindustryCodeString()
        {
            // sample  set a 2    set a b
            return $"set {Left.VariableOrValueString} {Right.VariableOrValueString}";
        }
    }
}
