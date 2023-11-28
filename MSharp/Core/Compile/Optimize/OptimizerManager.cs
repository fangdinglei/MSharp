using MSharp.Core.CodeAnalysis.Compile.MindustryCode;
using MSharp.Core.Compile.Language;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSharp.Core.Compile.Optimize
{
    internal abstract class BaseOptimizer
    {
        public abstract bool OnlyOneTime { get; }
        public abstract IReadOnlyList<BaseCode> LocalOptimize(IReadOnlyList<BaseCode> codes, IHelper helper);

        public class BiMatchResult
        {
            public List<BaseCode> ResultCodes;
            public bool DropAll;

            public BiMatchResult(List<BaseCode> resultCodes, bool dropAll = false)
            {
                ResultCodes = resultCodes;
                DropAll = dropAll;
            }

            public void Clear()
            {
                ResultCodes.Clear();
                DropAll = false;
            }
        }
        protected IReadOnlyList<BaseCode> BiMatch<T1, T2>(IReadOnlyList<BaseCode> codes, Action<T1, T2, BiMatchResult> func, IHelper helper)
                where T1 : BaseCode where T2 : BaseCode
        {
            List<BaseCode>? res = new List<BaseCode>();
            BiMatchResult context = new BiMatchResult(new List<BaseCode>());
            bool change = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (i + 1 >= codes.Count)
                {
                    res.Add(codes[i]);
                    continue;
                }
                if (typeof(T1) == codes[i].GetType() && typeof(T2) == codes[i + 1].GetType())
                {
                    func((T1)codes[i], (T2)codes[i + 1], context);
                    if (context.DropAll)
                    {
                        //drop all
                        change = true;
                        helper.OnDropCode(codes[i]);
                        helper.OnDropCode(codes[i + 1]);
                        context.Clear();
                        i++;
                    }
                    else if (context.ResultCodes.Count > 0)
                    {
                        // replace
                        change = true;
                        helper.OnDropCode(codes[i]);
                        helper.OnDropCode(codes[i + 1]);
                        res.AddRange(context.ResultCodes);
                        context.Clear();
                        i++;
                    }
                    else
                    {
                        // not changed
                        res.Add(codes[i]);
                    }
                }
                else
                {
                    res.Add(codes[i]);
                }
            }

            return change ? res : codes;
        }
    }

    internal class SensorOptimizer : BaseOptimizer
    {
        public override bool OnlyOneTime => true;

        public override IReadOnlyList<BaseCode> LocalOptimize(IReadOnlyList<BaseCode> codes, IHelper helper)
        {
            return BiMatch<Code_Sensor, Code_Assign>(codes, (a, b, context) =>
            {
                if (a.Result.Variable != b.Right.Variable!)
                    return;
                if (!helper.NoReadExceptThis(b, a.Result.Variable!))
                    return;
                context.ResultCodes.Add(new Code_Sensor(b.Left, a.Object, a.Member));
            }, helper);
        }
    }
    internal class JumpOptimizer : BaseOptimizer
    {
        public override bool OnlyOneTime => true;

        public override IReadOnlyList<BaseCode> LocalOptimize(IReadOnlyList<BaseCode> codes, IHelper helper)
        {
            /**
             * [logic op] [result] [var1] [var2]
             * jump [line] notEqual [result] 1
             * ===>
             * jump [line] [ ! logic op ] [var1] [var2]
             * or ===>
             * jump [line] always
             * or ===>
             * drop if always false
             */
            return BiMatch<Code_Operation, Code_Jump>(codes, (a, b, context) =>
            {
                if (b.Op != Code_Jump.OpCode.notEqual)
                    return;
                if (b.Right == null || b.Right.Value == null || b.Right.Value is not int || 1 != (int)b.Right.Value)
                    return;
                if (b.Left == null || a.Result.Variable != b.Left.Variable!)
                    return;
                if (a.Operation == MindustryOperatorKind.strictEqual)
                    throw new Exception("not supported");
                if (!helper.NoReadExceptThis(b, a.Result.Variable))
                {
                    return;
                }
                if (a.Left.IsValue && a.Right != null && a.Right.IsValue)
                {
                    // check const
                    var succ = helper.ConstReduce(a.Left.Value, a.Right.Value, a.Operation, out var c);
                    if (succ && c is int i)
                    {
                        if (i == 0)
                            context.ResultCodes.AddThen(new Code_Jump(Code_Jump.OpCode.always, null, null))
                                .JumpTo(b.To!);
                        else
                            context.DropAll = true;

                    }
                }
                else
                {
                    // invert
                    var rop = (MindustryOperatorKind)((int)a.Operation ^ 1);
                    context.ResultCodes.AddThen(new Code_Jump(helper.Convert(rop), a.Left, a.Right))
                      .JumpTo(b.To!);
                }


            }, helper);
        }
    }

    interface IHelper
    {
        bool NoReadExceptThis(BaseCode code, LVariable variable);
        void OnDropCode(BaseCode code);
        Code_Jump.OpCode Convert(MindustryOperatorKind op);
        bool ConstReduce(object? v1, object? v2, MindustryOperatorKind op, out object? res);
    }
    internal class OptimizerManager
    {
        List<BaseOptimizer> _optimizers = new List<BaseOptimizer>()
        {
            new  SensorOptimizer(),
            new JumpOptimizer(),
        };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="codes"></param>
        /// <returns></returns>
        public IReadOnlyList<BaseCode> Optimize(IReadOnlyList<BaseCode> codes)
        {
            HelperImp helper = new HelperImp();
            // Continuous, non jump in codes
            List<BaseCode> block = new List<BaseCode>();
            List<BaseCode> result = new List<BaseCode>();

            for (int i = 0; i < codes.Count; i++)
            {
                var code = codes[i];
                // no jump in or first code
                if (code.JumpFrom.Count == 0 || block.Count == 0)
                {
                    block.Add(code);
                }
                else if (block.Count > 1)
                {
                    var newBlock = LocalOptimize(block, helper);

                    if (newBlock.Count == 0)
                    {
                        if (block[0].JumpFrom.Count > 0)
                        {
                            if (i >= codes.Count)
                                throw new Exception("jump to null,need end code");
                            codes[i].JumpFrom.AddRange(block[0].JumpFrom);
                        }
                    }
                    else
                    {
                        result.AddRange(newBlock);
                        if (block[0].JumpFrom.Count > 0)
                        {
                            newBlock[0].JumpFrom.AddRange(block[0].JumpFrom);
                        }
                    }
                    i = i - 1;
                    block.Clear();
                }
            }

            if (block.Count > 1)
            {
                var newBlock = LocalOptimize(block, helper);

                if (newBlock.Count == 0)
                {
                    if (block[0].JumpFrom.Count > 0)
                    {
                        throw new Exception("jump to null,need end code");
                    }
                }
                else
                {
                    result.AddRange(newBlock);
                    if (block[0].JumpFrom.Count > 0)
                    {
                        newBlock[0].JumpFrom.AddRange(block[0].JumpFrom);
                    }
                }
                block.Clear();
            }

            return result;
        }

        private IReadOnlyList<BaseCode> LocalOptimize(IReadOnlyList<BaseCode> block, IHelper helper)
        {
            int times = 0;
            bool changed = true;
            while (changed)
            {
                changed = false;
                foreach (var optimizer in _optimizers)
                {
                    if (times != 0 && optimizer.OnlyOneTime)
                        continue;
                    var newBlock = optimizer.LocalOptimize(block, helper);
                    if (newBlock == block)
                        continue;
                    changed = true;
                    block = newBlock;
                }
                times++;
            }
            return block;
        }
    }

    internal class HelperImp : IHelper
    {
        public List<BaseCode> Codes = new List<BaseCode>();
        public bool ConstReduce(object? v1, object? v2, MindustryOperatorKind op, out object? res)
        {
            res = null;
            //TODO
            return false;
        }

        public Code_Jump.OpCode Convert(MindustryOperatorKind op)
        {
            switch (op)
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
                    return Code_Jump.OpCode.equal;
                case MindustryOperatorKind.notEqual:
                    return Code_Jump.OpCode.notEqual;
                case MindustryOperatorKind.land:
                    break;
                case MindustryOperatorKind.lessThanEq:
                    return Code_Jump.OpCode.lessThanEq;
                case MindustryOperatorKind.greaterThan:
                    return Code_Jump.OpCode.greaterThan;
                case MindustryOperatorKind.greaterThanEq:
                    return Code_Jump.OpCode.greaterThanEq;
                case MindustryOperatorKind.lessThan:
                    return Code_Jump.OpCode.lessThan;
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
            throw new Exception("not supported");
        }

        public void OnDropCode(BaseCode code)
        {
            if (code is Code_Jump jump && jump.To != null)
            {
                jump.To.JumpFrom.Remove(jump);
            }
        }

        private HashSet<BaseCode> _noReadExceptThisCache = new HashSet<BaseCode>();
        public bool NoReadExceptThis(BaseCode code, LVariable variable)
        {
            if (_noReadExceptThisCache.Contains(code))
                return true;
            foreach (var cd in Codes)
            {
                if (cd == code)
                    continue;
                if (cd.VariablesRead.Any(it => it.Variable == variable))
                {
                    return false;
                }
            }
            _noReadExceptThisCache.Add(code);
            return true;
        }
    }

}
