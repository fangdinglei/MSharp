using MSharp.Core.CodeAnalysis.MindustryCode;
using System.Collections.Generic;

namespace MSharp.Core.CodeAnalysis
{
    internal interface ICompiler
    {

        List<BaseCode> Compile(params CodeFile[] codes);

        string CompileToText(params CodeFile[] codes);

    }
}
