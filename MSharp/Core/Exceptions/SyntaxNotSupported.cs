using Microsoft.CodeAnalysis.CSharp;
using System;

namespace MSharp.Core.Exceptions
{
    public class CompileError : Exception
    {
        public CompileError(string? message) : base(message)
        {

        }
    }

    public class SyntaxNotSupported : CompileError
    {
        public SyntaxNotSupported(string tip) : base(tip)
        {

        }

        public SyntaxNotSupported(CSharpSyntaxNode syntax)
            : base($"SyntaxNotSupported:{syntax}")
        {

        }

        public SyntaxNotSupported(string tip, CSharpSyntaxNode syntax)
           : base($"{tip}:{syntax}")
        {

        }
    }
}
