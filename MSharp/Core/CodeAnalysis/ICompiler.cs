using MSharp.Core.CodeAnalysis.MindustryCode;
using System.Collections.Generic;

namespace MSharp.Core.CodeAnalysis
{
    internal interface ICompiler
    {

        List<BaseCode> Compile(params string[] codes);

        string CompileToText(params string[] codes);

    }
}
