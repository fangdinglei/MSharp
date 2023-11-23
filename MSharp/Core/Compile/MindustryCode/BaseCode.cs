using MSharp.Core.Compile.Language;
using System.Collections.Generic;
using System.Linq;

namespace MSharp.Core.Compile.MindustryCode
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
        protected HashSet<LVariableOrValue> _variables = new HashSet<LVariableOrValue>();
        public IReadOnlyList<LVariableOrValue> Variables => _variables.ToList();
        public int Index = -1;
        public virtual int CodeLength { get; } = 1;
        public bool Deprecated;


        public BaseCode DeepClone()
        {
            throw new System.NotImplementedException();
        }

        public abstract string ToMindustryCodeString();

        public override string ToString()
        {
            return ToMindustryCodeString();
        }
    }
}
