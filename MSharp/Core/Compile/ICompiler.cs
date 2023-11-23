using MSharp.Core.Compile.MindustryCode;
using System.Collections.Generic;

namespace MSharp.Core.Compile
{
    internal interface ICompiler
    {

        List<BaseCode> Compile(params CodeFile[] codes);

        string CompileToText(params CodeFile[] codes);

    }
}
