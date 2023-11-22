using MSharp.Core.CodeAnalysis.Language;

namespace MSharp.Core.CodeAnalysis.MindustryCode
{
    internal class Code_Operation : BaseCode
    {
        public readonly MindustryOperatorKind Operation;
        public readonly LVariableOrValue Left;
        public readonly LVariableOrValue? Right;
        public readonly LVariableOrValue Result;
        public Code_Operation(MindustryOperatorKind op, LVariableOrValue result, LVariableOrValue op1, LVariableOrValue op2)
        {
            Operation = op;
            Left = op1;
            Right = op2;
            Result = result;
            _variables.Add(op1);
            _variables.Add(op2);
            _variables.Add(Result);
        }
        public override string ToMindustryCodeString()
        {
            // sample  op mul val num1 num2
            var r = Right == null ? string.Empty : " " + Right!.VariableOrValueString;
            return $"op {Operation} {Result!.VariableOrValueString} {Left!.VariableOrValueString}{r}";
        }
    }
}
