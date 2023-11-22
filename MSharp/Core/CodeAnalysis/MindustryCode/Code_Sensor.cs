using MSharp.Core.CodeAnalysis.Language;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.MindustryCode
{
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
