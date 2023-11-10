using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace MSharp.Core.CodeAnalysis
{

    internal class LClass
    {
        public readonly CompileContext Context;

        public readonly INamedTypeSymbol Symbol;
        /// <summary>
        /// 连接到处理器的建筑名称集合(通过代码推断出的)
        /// </summary>
        public readonly List<string> Connects = new List<string>();
        /// <summary>
        /// 类中的函数
        /// </summary>
        public readonly Dictionary<IMethodSymbol, LMethod> Functions = new(SymbolEqualityComparer.Default);

        public LClass(CompileContext compileContext, INamedTypeSymbol symbol)
        {
            Context = compileContext;
            Symbol = symbol;
        }

        public LMethod CreateMethod(IMethodSymbol symbol)
        {
            var res = new LMethod(this, symbol);
            Functions.Add(symbol, res);
            Context.AddMethod(symbol, res);
            return res;
        }
    }

    internal class LVariable
    {
        public LBlock Block;
        public int? Index;
        public string? Name;
        public TypeInfo Type;

        public bool IsTemp;

        public string RealName => (Index.HasValue ? "var" + Index : Name);

        public LVariable(LBlock block, string name, TypeInfo type)
        {
            Block = block;
            Name = name;
            Type = type;
        }

        public LVariable(LBlock block, int index, TypeInfo type)
        {
            Block = block;
            Index = index;
            Type = type;
            IsTemp = true;
        }

        public LVariable(LBlock block, string name, int index, TypeInfo type)
        {
            Block = block;
            Index = index;
            Name = name;
            Type = type;
            IsTemp = false;
        }



        public override string ToString()
        {
            return Type + " " + RealName;
        }
    }
    internal class LMethod
    {
        public LClass Parent;
        public IMethodSymbol Symbol;

        public MethodCallMode CallMode;

        public LBlock? Block;

        public LMethod(LClass parent, IMethodSymbol symbol)
        {
            Parent = parent;
            Symbol = symbol;
        }



    }
    internal class LBlock
    {
        [Obsolete]
        public readonly LBlock? Parent;
        /// <summary>
        /// 代码
        /// </summary>
        public readonly List<BaseCode> Codes = new List<BaseCode>();
        /// <summary>
        /// 变量表
        /// </summary>
        public VariableTable VariableTable;

        /// <summary>
        /// custom methods called in block 调用的自定义函数
        /// </summary>
        public SortedSet<LMethod> Calls = new SortedSet<LMethod>();

        public LBlock(LBlock? parent)
        {
            Parent = parent;
            VariableTable = new VariableTable(this);
        }

        /// <summary>
        /// merge another block to this
        /// </summary>
        /// <param name="block">block to be merged</param>
        /// <param name="parametersDict">formal->actual parameters 形参->实参</param>
        public void Merge(LBlock block, Dictionary<LVariable, LVariable>? parametersDict)
        {
            // old->new parameters 旧->新变量映射表
            Dictionary<LVariable, LVariable> newVariablesDict;
            if (parametersDict != null)
                newVariablesDict = new(parametersDict);
            else
                newVariablesDict = new();
            // To prevent conflicts, reset the variable name 要将所有变量重写，防止冲突
            foreach (LVariable variable in block.VariableTable.GetAll())
            {
                var newVariable = this.VariableTable.Add(variable);
                newVariablesDict.Add(variable, newVariable);
            }

            // reset the variable name of codes 重写变量名
            foreach (var code in block.Codes)
            {
                var newCodeVariables = code.Variables.Select(it =>
                {
                    if (newVariablesDict.TryGetValue(it, out var res))
                        return res;
                    Debug.Assert(false);
                    return it;// TODO 不应该出现不在变量表中
                }).ToList();
                Codes.Add(code.Clone(newCodeVariables));
            }
        }

    }

    internal class VariableTable
    {
        int ptr = 0;
        Dictionary<string, LVariable> defines = new();
        LBlock _block;

        public VariableTable(LBlock block)
        {
            _block = block;
        }

        public LVariable Add(TypeInfo type, string name)
        {
            while (true)
            {
                var v = new LVariable(_block, name, type);
                if (defines.ContainsKey(v.RealName))
                    continue;
                defines.Add(v.RealName, v);
                return v;
            }
        }

        public LVariable Add(TypeInfo type)
        {
            while (true)
            {
                var v = new LVariable(_block, ++ptr, type);
                if (defines.ContainsKey(v.RealName))
                    continue;
                defines.Add(v.RealName, v);
                return v;
            }
        }

        public LVariable Add(LVariable variable)
        {
            if (defines.ContainsKey(variable.RealName) && !variable.IsTemp)
            {
                Debug.Assert(variable.Name != null);
                while (true)
                {
                    var v = new LVariable(_block, variable.Name, ++ptr, variable.Type);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    return v;
                }
            }
            else
            {
                while (true)
                {
                    var v = new LVariable(_block, ++ptr, variable.Type);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    return v;
                }
            }
            throw new Exception();
        }

        public ICollection<LVariable> GetAll()
        {
            return defines.Values;
        }

    }

}
