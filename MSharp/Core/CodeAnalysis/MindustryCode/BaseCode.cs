using MSharp.Core.CodeAnalysis.Language;
using System.Collections.Generic;
using System.Linq;

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
        protected HashSet<LVariableOrValue> _variables = new HashSet<LVariableOrValue>();
        public IReadOnlyList<LVariableOrValue> Variables => _variables.ToList();
        public int Index = -1;


        public BaseCode DeepClone()
        {
            throw new System.NotImplementedException();
        }

        public abstract string ToMindustryCodeString();

    }
}
