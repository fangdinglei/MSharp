using MSharp.Core.CodeAnalysis.Compile.MindustryCode;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace MSharp.Core.Simulate
{
    internal class MVM
    {
        private List<BaseCode> _codes;
        public Dictionary<string, object> Values = new Dictionary<string, object>();

        public MVM(List<BaseCode> codes)
        {
            _codes = codes;
        }

        private dynamic GetValue(LVariableOrValue vv)
        {
            if (vv.IsVariable)
                return Values[vv.VariableOrValueString];
            else
                return vv.Value!;
        }
        private object SetValue(LVariableOrValue vv,object obj )
        {
            if (obj is bool b)
                obj = b ? 1 : 0;
            if (vv.IsVariable)
                 Values[vv.VariableOrValueString]=obj;
            else
                throw new Exception();
            return obj;
        }
        public void Run()
        {
            int ptr = 0;
            while (ptr < _codes.Count)
            {
                var code = _codes[ptr++];
                if (code is Code_Assign assign)
                {
                    SetValue(assign.Left, assign.Right);
                }
                else if (code is Code_Jump jump)
                {
                    switch (jump.Op)
                    {
                        case Code_Jump.OpCode.equal:
                            ptr = GetValue(jump.Left) == GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.notEqual:
                            ptr = GetValue(jump.Left) != GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.greaterThanEq:
                            ptr = GetValue(jump.Left) >= GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.lessThan:
                            ptr = GetValue(jump.Left) < GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.lessThanEq:
                            ptr = GetValue(jump.Left) <= GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.greaterThan:
                            ptr = GetValue(jump.Left) > GetValue(jump.Right) ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.always:
                            ptr = true ? jump.To.Index : ptr;
                            break;
                        case Code_Jump.OpCode.strictEqual:
                            throw new Exception("not supported");
                            break;
                        default:
                            break;
                    }
                }
                else if (code is Code_Operation operations)
                {
                    switch (operations.Operation)
                    {
                        case MindustryOperatorKind.add:
                            break;
                        case MindustryOperatorKind.sub:
                            break;
                        case MindustryOperatorKind.mul:
                            break;
                        case MindustryOperatorKind.div:
                            break;
                        case MindustryOperatorKind.idiv:
                            break;
                        case MindustryOperatorKind.mod:
                            break;
                        case MindustryOperatorKind.pow:
                            break;
                        case MindustryOperatorKind.equal:
                            SetValue(operations.Result, GetValue(operations.Left) == GetValue(operations.Right));
                            break;
                        case MindustryOperatorKind.notEqual:
                            break;
                        case MindustryOperatorKind.land:
                            break;
                        case MindustryOperatorKind.lessThanEq:
                            break;
                        case MindustryOperatorKind.greaterThan:
                            break;
                        case MindustryOperatorKind.greaterThanEq:
                            break;
                        case MindustryOperatorKind.lessThan:
                            break;
                        case MindustryOperatorKind.shl:
                            break;
                        case MindustryOperatorKind.shr:
                            break;
                        case MindustryOperatorKind.or:
                            break;
                        case MindustryOperatorKind.and:
                            break;
                        case MindustryOperatorKind.xor:
                            break;
                        case MindustryOperatorKind.not:
                            break;
                        case MindustryOperatorKind.max:
                            break;
                        case MindustryOperatorKind.min:
                            break;
                        case MindustryOperatorKind.angle:
                            break;
                        case MindustryOperatorKind.len:
                            break;
                        case MindustryOperatorKind.noise:
                            break;
                        case MindustryOperatorKind.abs:
                            break;
                        case MindustryOperatorKind.log:
                            break;
                        case MindustryOperatorKind.log10:
                            break;
                        case MindustryOperatorKind.sin:
                            break;
                        case MindustryOperatorKind.cos:
                            break;
                        case MindustryOperatorKind.tan:
                            break;
                        case MindustryOperatorKind.floor:
                            break;
                        case MindustryOperatorKind.ceil:
                            break;
                        case MindustryOperatorKind.sqrt:
                            break;
                        case MindustryOperatorKind.rand:
                            break;
                        case MindustryOperatorKind.strictEqual:
                            break;
                        default:
                            break;
                    }
                }
            }
        }

    }
}
