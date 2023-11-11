using Microsoft.CodeAnalysis;

namespace MSharp.Core.CodeAnalysis.Compile.Method.ExpressionHandles
{
    internal abstract class ExpressionHandle
    {
        public abstract LVariableOrValue GetValue(SemanticModel semantic, LMethod method, LBlock block);
    }
}
