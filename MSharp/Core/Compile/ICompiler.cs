using MSharp.Core.Compile.MindustryCode;
using System.Collections.Generic;

namespace MSharp.Core.Compile
{
    internal interface ICompiler
    {

        List<BaseCode> Compile(params CodeFile[] codes);
        void ReIndex(List<BaseCode> codes);
        string CompileToText(List<BaseCode> codes);
        string CompileToText(params CodeFile[] codes);

    }
}
