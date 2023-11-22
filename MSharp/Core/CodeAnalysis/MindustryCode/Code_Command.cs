using MSharp.Core.CodeAnalysis.Language;

namespace MSharp.Core.CodeAnalysis.MindustryCode
{
    internal class Code_Command : BaseCode
    {
        public readonly string Name;
        public readonly LVariableOrValue Value;

        public Code_Command(string name, LVariableOrValue value)
        {
            Name = name;
            Value = value;
        }
        public override string ToMindustryCodeString()
        {
            return $"{Name} {Value.VariableOrValueString}";
        }
    }
}
