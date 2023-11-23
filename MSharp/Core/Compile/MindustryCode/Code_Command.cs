using MSharp.Core.Compile.Language;

namespace MSharp.Core.Compile.MindustryCode
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
