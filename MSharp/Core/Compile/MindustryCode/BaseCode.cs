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
        private HashSet<LVariableOrValue> _variables = new HashSet<LVariableOrValue>();
        private HashSet<LVariableOrValue> _variablesRead = new HashSet<LVariableOrValue>();
        private HashSet<LVariableOrValue> _variablesWrite = new HashSet<LVariableOrValue>();

        public IReadOnlyList<LVariableOrValue> Variables => _variables.ToList();
        public IReadOnlyList<LVariableOrValue> VariablesRead => _variablesRead.ToList();
        public IReadOnlyList<LVariableOrValue> VariablesWrite => _variablesWrite.ToList();
        public int Index = -1;
        public virtual int CodeLength { get; } = 1;
        public bool Deprecated;

        protected LVariableOrValue? R(LVariableOrValue? r) {
            if (r == null)
                return r;
            _variablesRead.Add(r);
            _variables.Add(r);
            return r;
        }
        protected LVariableOrValue? W(LVariableOrValue? w)
        {
            if (w == null)
                return w;
            _variablesWrite.Add(w);
            _variables.Add(w);
            return w;
        }

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
