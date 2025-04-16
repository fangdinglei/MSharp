using MSharp.Core.Compile.Language;

namespace MSharp.Core.Compile.MindustryCode
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
            R(op1);
            R(op2);
            W(Result);
        }
        public override string ToMindustryCodeString()
        {
            // sample  op mul val num1 num2
            var r = Right == null ? string.Empty : " " + Right!.VariableOrValueString;
            return $"op {Operation} {Result!.VariableOrValueString} {Left!.VariableOrValueString}{r}";
        }
    }
}
