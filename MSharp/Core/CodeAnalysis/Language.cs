using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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

    /// <summary>
    /// 变量 一个同样的变量应该具有同样的引用
    /// </summary>
    internal class LVariable
    {
        public LMethod Method;
        public int? Index;
        public string? Name;
        public ITypeSymbol Type;
        public ISymbol? Symbol;

        public bool IsTemp;

        public string RealName => (Index.HasValue ? "var" + Index : Name);

        public LVariable(LMethod method, string name, ITypeSymbol type,ISymbol symbol)
        {
            Method = method;
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public LVariable(LMethod method, int index, ITypeSymbol type)
        {
            Method = method;
            Index = index;
            Type = type;
            IsTemp = true;
        }

        public LVariable(LMethod method, string name, int index, ITypeSymbol type, ISymbol? symbol)
        {
            Method = method;
            Index = index;
            Name = name;
            Type = type;
            IsTemp = false;
            Symbol = symbol;
        }



        public override string ToString()
        {
            return Type + " " + RealName;
        }
    }
    internal class LVariableOrValue
    {
        public LVariable? Variable;

        public object? Value;

        public bool IsVariable => Variable != null;

#pragma warning disable CS8603
        public string VariableOrValueString => Variable != null ? Variable.RealName : Value!.ToString();
#pragma warning restore CS8603

        public LVariableOrValue(object? value)
        {
            Debug.Assert(value is not LVariable);
            Value = value;
        }
        public LVariableOrValue(LVariable? variable)
        {
            Variable = variable;
        }
    }

    internal class LMethod
    {
        public LClass Parent;
        public IMethodSymbol Symbol;

        public MethodCallMode CallMode;
        public List<LParameter>? Parameters;

        public LBlock? Block;

        public LVariableOrValue? Return;



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
        /// merge another method to this
        /// </summary>
        /// <param name="method">method to be merged</param>
        /// <param name="parametersDict">formal->actual parameters 形参->实参</param>
        public void Merge(LMethod method, Dictionary<LVariable, LVariableOrValue> parametersDict)
        {
            Debug.Assert(this.Block != null && method.Block != null);
            // old->new parameters 旧->新变量映射表
            Dictionary<LVariable, LVariable> newVariablesDict = new();
            // To prevent conflicts, reset the variable name 要将所有变量重写，防止冲突
            foreach (LVariable variable in method.VariableTable.GetAll())
            {
                var newVariable = VariableTable.Add(variable);
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
                        it.Variable = res;
                    else if (parametersDict.TryGetValue(it.Variable, out var res2))
                        it.Value = res2.Value;
                    Debug.Assert(false);// TODO 不应该出现不在变量表中
                });
                Block.Codes.Add(newCode);
            }
        }

        public void Emit(BaseCode baseCode)
        {
            Console.WriteLine("emit " + baseCode.ToMindustryCodeString());
        }
    }
    internal class LBlock
    {
        public readonly LMethod Method;
        /// <summary>
        /// 代码
        /// </summary>
        public readonly List<BaseCode> Codes = new List<BaseCode>();

        /// <summary>
        /// custom methods called in method 调用的自定义函数
        /// </summary>
        public SortedSet<LMethod> Calls = new SortedSet<LMethod>();

        public LBlock(LMethod method)
        {
            Method = method;
        }

    }

    internal class VariableTable
    {
        int ptr = 0;
        Dictionary<string, LVariable> defines = new();
        Dictionary<ISymbol, LVariable> SymbolDict = new( SymbolEqualityComparer.Default);
        LMethod _method;

        public VariableTable(LMethod method)
        {
            _method = method;
        }

        public LVariable Add(ITypeSymbol type,ISymbol symbol, string name)
        {
            while (true)
            {
                var v = new LVariable(_method, name, type, symbol);
                if (defines.ContainsKey(v.RealName))
                    continue;
                defines.Add(v.RealName, v);
                SymbolDict.Add(symbol, v);
                return v;
            }
        }

        public LVariable Add(ITypeSymbol type)
        {
            while (true)
            {
                var v = new LVariable(_method, ++ptr, type);
                if (defines.ContainsKey(v.RealName))
                    continue;
                defines.Add(v.RealName, v);
                return v;
            }
        }

        public LVariable Add(LVariable variable)
        {
            if (!variable.IsTemp)
            {
                Debug.Assert(variable.Name != null);
                // 如果没有，直接使用原名
                if (!defines.ContainsKey(variable.RealName))
                {
                    defines.Add(variable.RealName, variable);
                    return variable;
                }
                // 使用自动生成的名称
                while (true)
                {
                    var v = new LVariable(_method, variable.Name, ++ptr, variable.Type, variable.Symbol);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    return v;
                }
            }
            else
            {
                // 生成临时变量
                while (true)
                {
                    var v = new LVariable(_method, ++ptr, variable.Type);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
#pragma warning disable CS8604
                    SymbolDict.Add(v.Symbol, v);
#pragma warning restore CS8604
                    return v;
                }
            }
            throw new Exception();
        }

        public LVariable Get(ISymbol symbol) {
            //TODO
            return SymbolDict[symbol];
        }

        public ICollection<LVariable> GetAll()
        {
            return defines.Values;
        }

    }

    internal class LParameter
    {
        public bool? Used = true;
        public object? DefaultValue;
        public LVariable Variable;

        public LParameter(LVariable variable, object? defaultValue)
        {
            Variable = variable;
            DefaultValue = defaultValue;
        }
    }
}
