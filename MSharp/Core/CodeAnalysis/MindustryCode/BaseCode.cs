using System.Collections.Generic;

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
        public abstract IReadOnlyList<LVariable> Variables { get;protected  set; }

        public BaseCode Clone(List<LVariable> variables) { 
            
        }

        public abstract string ToMindustryCodeString();
    }


}
