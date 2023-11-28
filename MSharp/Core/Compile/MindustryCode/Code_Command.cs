using MSharp.Core.Compile.Language;
using System.Diagnostics;

namespace MSharp.Core.Compile.MindustryCode
{
    internal class Code_Command : BaseCode
    {
        public readonly string Name;
        public readonly LVariableOrValue Value;

        public Code_Command(string name, LVariableOrValue value, bool[] read)
        {
            Debug.Assert(name != null);
            Debug.Assert(value != null && value.ValueList != null);
            Debug.Assert(read.Length == value.ValueList.Count);
            Name = name;
            Value = value;
            for (int i = 0; i < value.ValueList!.Count; i++)
            {
                var item = value.ValueList![i];
                var r = read[i];
                if (r)
                    R(item);
                else
                    W(item);
            }
        }
        public override string ToMindustryCodeString()
        {
            return $"{Name} {Value.VariableOrValueString}";
        }
    }
}
