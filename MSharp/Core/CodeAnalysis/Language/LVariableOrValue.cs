using System.Collections.Generic;
using System.Diagnostics;

namespace MSharp.Core.CodeAnalysis.Language
{
    internal class LVariableOrValue
    {
        static public LVariableOrValue ZERO = new LVariableOrValue(0);
        static public LVariableOrValue ONE = new LVariableOrValue(1);
        static public LVariableOrValue VOID = new LVariableOrValue((object?)null);

        public LVariable? Variable { get; private set; }

        public object? Value { get; private set; }

        public List<LVariableOrValue>? ValueList { get; private set; }

        public bool IsVariable => Variable != null;

        public bool IsList => ValueList != null;

        public string VariableOrValueString => IsVariable ? Variable!.RealName
            : IsList ? MultiValueToString()
            : ConvertToInt(Value).ToString()!;


        public LVariableOrValue(object? value)
        {
            Debug.Assert(value is not LVariable);
            Debug.Assert(value is not LVariableOrValue);
            Debug.Assert(value is not List<LVariableOrValue>);
            Value = value;
        }
        public LVariableOrValue(LVariable variable)
        {
            Variable = variable;
        }
        public LVariableOrValue(List<LVariableOrValue> variables, int pad)
        {
            ValueList = variables;
            while (variables.Count < pad)
                variables.Add(ZERO);
        }

        public void Replace(object? obj)
        {
            if (obj is LVariable a)
                Variable = a;
            else if (obj is List<LVariableOrValue> b)
                ValueList = b;
            else
                Value = obj;
        }

        protected string MultiValueToString()
        {
            string res = "";
            foreach (LVariableOrValue obj in ValueList!)
                res += obj.VariableOrValueString + " ";
            return res.TrimEnd();
        }
        protected object ConvertToInt(object? obj)
        {
            if (obj is bool b)
                return b ? 1 : 0;
            if (obj == null)
                return 0;
            return obj;
        }
        public override string ToString()
        {
            return VariableOrValueString;
        }
    }
}
