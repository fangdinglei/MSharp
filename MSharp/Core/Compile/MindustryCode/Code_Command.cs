using MSharp.Core.Compile.Language;
using MSharp.Core.Utility;

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
            value.ValueList!.ForEach(_variables.AddIfNotNullNoReturn);
        }
        public override string ToMindustryCodeString()
        {
            return $"{Name} {Value.VariableOrValueString}";
        }
    }
}
