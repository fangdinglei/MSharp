using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile.MindustryCode;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSharp.Core.Simulate
{
    internal abstract class BaseCodeExecuter
    {
        public abstract Type MatchType { get; }
        public abstract void Execute(object code, MThread thread, IVariableTable variableTable);
    }
    /// <summary>
    /// dynamic used by <see cref="MVM"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class BaseCodeExecuter<T> : BaseCodeExecuter where T : BaseCode
    {
        public override Type MatchType => typeof(T);

        public override void Execute(object code, MThread thread, IVariableTable variableTable)
        {
            Execute((T)code, thread, variableTable);
        }
        public abstract void Execute(T code, MThread thread, IVariableTable variableTable);
    }

    internal class AssignExecuter : BaseCodeExecuter<Code_Assign>
    {
        public override void Execute(Code_Assign assign, MThread thread, IVariableTable variableTable)
        {
            variableTable.SetValue(assign.Left, variableTable.GetValue(assign.Right));
        }
    }

    internal class JumpExecuter : BaseCodeExecuter<Code_Jump>
    {
        public override void Execute(Code_Jump jump, MThread thread, IVariableTable vt)
        {
            switch (jump.Op)
            {
                case Code_Jump.OpCode.equal:
                    thread.CodePtr = vt.GetValue(jump.Left) == vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.notEqual:
                    thread.CodePtr = vt.GetValue(jump.Left) != vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.greaterThanEq:
                    thread.CodePtr = vt.GetValue(jump.Left) >= vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.lessThan:
                    thread.CodePtr = vt.GetValue(jump.Left) < vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.lessThanEq:
                    thread.CodePtr = vt.GetValue(jump.Left) <= vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.greaterThan:
                    thread.CodePtr = vt.GetValue(jump.Left) > vt.GetValue(jump.Right) ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.always:
                    thread.CodePtr = true ? jump.To.Index : thread.CodePtr;
                    break;
                case Code_Jump.OpCode.strictEqual:
                    throw new Exception("not supported");
                default:
                    break;
            }
        }
    }

    internal class OperationExecuter : BaseCodeExecuter<Code_Operation>
    {
        public override void Execute(Code_Operation operation, MThread thread, IVariableTable vt)
        {
            var getValue = vt.GetValue;
            var setValue = vt.SetValue;
            switch (operation.Operation)
            {
                case MindustryOperatorKind.add:
                    setValue(operation.Result, getValue(operation.Left) + getValue(operation.Right));
                    break;
                case MindustryOperatorKind.sub:
                    setValue(operation.Result, getValue(operation.Left) - getValue(operation.Right));
                    break;
                case MindustryOperatorKind.mul:
                    setValue(operation.Result, getValue(operation.Left) * getValue(operation.Right));
                    break;
                case MindustryOperatorKind.div:
                    setValue(operation.Result, getValue(operation.Left) * 1d / getValue(operation.Right));
                    break;
                case MindustryOperatorKind.idiv:
                    setValue(operation.Result, (int)(getValue(operation.Left) / getValue(operation.Right)));
                    break;
                case MindustryOperatorKind.mod:
                    setValue(operation.Result, (int)(getValue(operation.Left) % getValue(operation.Right)));
                    break;
                case MindustryOperatorKind.pow:
                    setValue(operation.Result, Math.Pow(getValue(operation.Left), getValue(operation.Right)));
                    break;
                case MindustryOperatorKind.equal:
                    setValue(operation.Result, getValue(operation.Left) == getValue(operation.Right));
                    break;
                case MindustryOperatorKind.notEqual:
                    setValue(operation.Result, getValue(operation.Left) != getValue(operation.Right));
                    break;
                case MindustryOperatorKind.land:
                    setValue(operation.Result, getValue(operation.Left) && getValue(operation.Right));
                    break;
                case MindustryOperatorKind.lessThanEq:
                    setValue(operation.Result, getValue(operation.Left) <= getValue(operation.Right));
                    break;
                case MindustryOperatorKind.greaterThan:
                    setValue(operation.Result, getValue(operation.Left) > getValue(operation.Right));
                    break;
                case MindustryOperatorKind.greaterThanEq:
                    setValue(operation.Result, getValue(operation.Left) >= getValue(operation.Right));
                    break;
                case MindustryOperatorKind.lessThan:
                    setValue(operation.Result, getValue(operation.Left) < getValue(operation.Right));
                    break;
                case MindustryOperatorKind.shl:
                    setValue(operation.Result, getValue(operation.Left) << getValue(operation.Right));
                    break;
                case MindustryOperatorKind.shr:
                    setValue(operation.Result, getValue(operation.Left) >> getValue(operation.Right));
                    break;
                case MindustryOperatorKind.or:
                    setValue(operation.Result, getValue(operation.Left) | getValue(operation.Right));
                    break;
                case MindustryOperatorKind.and:
                    setValue(operation.Result, getValue(operation.Left) & getValue(operation.Right));
                    break;
                case MindustryOperatorKind.xor:
                    setValue(operation.Result, getValue(operation.Left) ^ getValue(operation.Right));
                    break;
                case MindustryOperatorKind.not:
                    setValue(operation.Result, ~getValue(operation.Left));
                    break;
                case MindustryOperatorKind.max:
                    setValue(operation.Result, Math.Max(getValue(operation.Left), getValue(operation.Right)));
                    break;
                case MindustryOperatorKind.min:
                    setValue(operation.Result, Math.Min(getValue(operation.Left), getValue(operation.Right)));
                    break;
                case MindustryOperatorKind.angle:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.len:
                    setValue(operation.Result, Math.Sqrt(Math.Pow(getValue(operation.Left), 2) + Math.Pow(getValue(operation.Right), 2)));
                    break;
                case MindustryOperatorKind.noise:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.abs:
                    setValue(operation.Result, Math.Sqrt(Math.Pow(getValue(operation.Left), 2) + Math.Pow(getValue(operation.Right), 2)));
                    break;
                case MindustryOperatorKind.log:
                    setValue(operation.Result, Math.Log2(getValue(operation.Left)));
                    break;
                case MindustryOperatorKind.log10:
                    setValue(operation.Result, Math.Log10(getValue(operation.Left)));
                    break;
                case MindustryOperatorKind.sin:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.cos:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.tan:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.floor:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.ceil:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.sqrt:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.rand:
                    throw new NotImplementedException();
                    break;
                case MindustryOperatorKind.strictEqual:
                    throw new NotImplementedException();
                    break;
                default:
                    break;
            }
        }
    }

    internal class CommandExecuter : BaseCodeExecuter<Code_Command>
    {
        public override void Execute(Code_Command cmd, MThread thread, IVariableTable variableTable)
        {
            if (cmd.Name.Contains(ConstStringDefine.CMD_DEBUGGER))
            {
                var res = variableTable.GetValue(cmd.Variables[0]) == variableTable.GetValue(cmd.Variables[1]);
                Console.WriteLine($"assert:{(res ? "success" : "fail")}");
            }
        }
    }

    internal interface IVariableTable
    {
        dynamic GetValue(LVariableOrValue vv);
        object SetValue(LVariableOrValue vv, object obj);
    }

    internal class MThread
    {
        public int CodePtr = 0;
    }
    internal class MVM : IVariableTable
    {
        static Dictionary<Type, BaseCodeExecuter> s_baseCodeExecuters = new();


        private List<BaseCode> _codes;
        public Dictionary<string, object> Values = new Dictionary<string, object>();

        static MVM()
        {
            foreach (var t in typeof(MVM).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(BaseCodeExecuter))))
            {
                BaseCodeExecuter instance = (BaseCodeExecuter)Activator.CreateInstance(t)!;
                Type match = instance.MatchType;
                s_baseCodeExecuters.Add(match, instance);
            }
        }

        public MVM(List<BaseCode> codes)
        {
            _codes = codes;
        }

        public dynamic GetValue(LVariableOrValue vv)
        {
            if (vv.IsVariable)
                return Values[vv.VariableOrValueString];
            else
                return vv.Value!;
        }
        public object SetValue(LVariableOrValue vv, object obj)
        {
            if (obj is bool b)
                obj = b ? 1 : 0;
            if (vv.IsVariable)
                Values[vv.VariableOrValueString] = obj is LVariableOrValue vv2 ? GetValue(vv2) : obj;
            else
                throw new Exception();
            return obj;
        }
        public void Run()
        {
            MThread mThread = new MThread();
            while (mThread.CodePtr < _codes.Count)
            {
                var code = _codes[mThread.CodePtr++];
                if (s_baseCodeExecuters.TryGetValue(code.GetType(), out BaseCodeExecuter? exe) && exe != null)
                {
                    exe!.Execute(code, mThread, this);
                }
                else
                {
                    //throw new SimulateError($"code not supported:{code.ToMindustryCodeString()}");
                }
            }
        }

    }
}
