using Microsoft.CodeAnalysis;
using MSharp.Core.Shared;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MSharp.Core.Compile.Language
{
    internal class LMethod
    {
        SortedSet<LMethod> _callTo = new SortedSet<LMethod>();
        SortedSet<LMethod> _callFrom = new SortedSet<LMethod>();

        public LClass Parent { get; init; }
        public IMethodSymbol Symbol { get; init; }

        public MethodCallMode CallMode;
        public List<LParameter>? Parameters;

        public LBlock? Block;

        public LVariableOrValue? Return;

        public IReadOnlySet<LMethod> CallTo => _callTo;
        public IReadOnlySet<LMethod> CallFrom => _callFrom;


        /// <summary>
        /// 变量表
        /// </summary>
        public VariableTable VariableTable;

        public LMethod(LClass parent, IMethodSymbol symbol)
        {
            Parent = parent;
            Symbol = symbol;
            VariableTable = new VariableTable(this);
        }

        /// <summary>
        /// Record call relationships
        /// </summary>
        /// <param name="called"></param>
        public void RecordCallTo(LMethod called)
        {
            _callTo.Add(called);
            called._callFrom.Add(this);
        }

        /// <summary>
        /// merge another method to this
        /// </summary>
        /// <param name="method">method to be merged</param>
        /// <param name="parametersDict">formal->actual parameters 形参->实参</param>
        public void Merge(LMethod method, Dictionary<LVariable, LVariableOrValue> parametersDict)
        {
            Debug.Assert(Block != null && method.Block != null);
            // old->new parameters 旧->新变量映射表
            Dictionary<LVariable, LVariable> newVariablesDict = new();
            // To prevent conflicts, reset the variable name 要将所有变量重写，防止冲突
            foreach (LVariable variable in method.VariableTable.GetAll())
            {
                var newVariable = VariableTable.ReNameVariable(variable);
                newVariablesDict.Add(variable, newVariable);
            }

            // reset the variable name of codes 重写变量名
            foreach (var code in method.Block.Codes)
            {
                // 方法可能不止一次被内联，这里不能修改直接原本的数据
                var newCode = code.DeepClone();
                newCode.Variables
                    .Where(it => it.IsVariable)
                    .ToList().ForEach(it =>
                {
                    Debug.Assert(it.Variable != null);
                    if (newVariablesDict.TryGetValue(it.Variable, out var res))
                        it.Replace(res);
                    else if (parametersDict.TryGetValue(it.Variable, out var res2))
                        it.Replace(res);
                    Debug.Assert(false);// TODO 不应该出现不在变量表中
                });
                Block.Codes.Add(newCode);
            }
        }
    }
}
