using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile;
using MSharp.Core.CodeAnalysis.MindustryCode;
using MSharp.Core.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MSharp.Core.CodeAnalysis
{
    internal class LClass
    {
        private Dictionary<IMethodSymbol, LMethod> _functions = new(SymbolEqualityComparer.Default);

        public readonly CompileContext Context;
        /// <summary>
        /// 类的定义，暂时没用，先记着
        /// </summary>
        public readonly INamedTypeSymbol Symbol;
        /// <summary>
        /// 字段表（不管是属性还是字段都记这）
        /// </summary>
        public readonly FieldTable VariableTable;

        /// <summary>
        /// 类中的函数
        /// </summary>
        public IReadOnlyDictionary<IMethodSymbol, LMethod> Functions => _functions;

        public LClass(CompileContext compileContext, INamedTypeSymbol symbol)
        {
            Context = compileContext;
            Symbol = symbol;
            VariableTable = new FieldTable(this);
        }

        public LMethod CreateMethod(IMethodSymbol symbol)
        {
            var res = new LMethod(this, symbol);
            _functions.Add(symbol, res);
            Context.AddMethod(symbol, res);
            return res;
        }
    }
    /// <summary>
    /// 变量 一个同样的变量应该具有同样的引用
    /// </summary>
    internal class LVariable
    {
        public const string TEMP_PREFIX = "_t_var_";


        public enum VariableType
        {
            Temp,
            LocalVar,
            Field,
        }

        public LClass? Class;
        public LMethod? Method;
        public int? Index;
        public string? Name;
        public ITypeSymbol? Type;
        public ISymbol? Symbol;

        public readonly VariableType Kind;

        public string RealName => (Index.HasValue ? TEMP_PREFIX + Index : Name)!;

        public LVariable(LClass clazz, ITypeSymbol type, ISymbol symbol, string name)
        {
            Debug.Assert(clazz != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Class = clazz;
            Kind = VariableType.Field;
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public LVariable(LMethod method, string name, ITypeSymbol type, ISymbol symbol)
        {
            Debug.Assert(method != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Kind = VariableType.LocalVar;
            Method = method;
            Name = name;
            Type = type;
            Symbol = symbol;
        }

        public LVariable(LMethod method, int index, ITypeSymbol type)
        {
            Kind = VariableType.Temp;
            Method = method;
            Index = index;
            Type = type;
        }

        public LVariable(LMethod method, string name, int index, ITypeSymbol? type, ISymbol? symbol, VariableType kind)
        {
            Debug.Assert(method != null && name != null && !name.StartsWith(TEMP_PREFIX));
            Method = method;
            Index = index;
            Name = name;
            Type = type;
            Symbol = symbol;
            Kind = kind;
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

        List<LVariableOrValue>? ValueList;

        public bool IsVariable => Variable != null;

        public bool IsList => ValueList != null;

        public string VariableOrValueString => IsVariable ? Variable!.RealName
            : IsList ? MultiValueToString()
            : ConvertToInt(Value).ToString()!;

        static public LVariableOrValue ZERO = new LVariableOrValue(0);
        static public LVariableOrValue ONE = new LVariableOrValue(1);
        static public LVariableOrValue VOID = new LVariableOrValue((object?)null);

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

    internal class LMethod
    {
        public LClass Parent;
        public IMethodSymbol Symbol;

        public MethodCallMode CallMode;
        public List<LParameter>? Parameters;

        public LBlock? Block;

        public LVariableOrValue? Return;

        /// <summary>
        /// custom methods called in method 调用的自定义函数
        /// </summary>
        public SortedSet<LMethod> Calls = new SortedSet<LMethod>();


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
                        it.Variable = res;
                    else if (parametersDict.TryGetValue(it.Variable, out var res2))
                        it.Value = res2.Value;
                    Debug.Assert(false);// TODO 不应该出现不在变量表中
                });
                Block.Codes.Add(newCode);
            }
        }
    }
    internal class LBlock
    {
        public readonly LMethod Method;
        /// <summary>
        /// 代码
        /// </summary>
        public List<BaseCode> Codes = new List<BaseCode>();

        public Action<BaseCode> ReturnCall = (a) => { };

        public Action<BaseCode> ContinueCall = (a) => { };

        public Action<BaseCode> NextCall = (a) => { };

        /// <summary>
        /// 主要是 i++这样的
        /// </summary>
        public List<BaseCode> PostCodes = new List<BaseCode>();

        public LBlock(LMethod method)
        {
            Method = method;
        }

        /// <summary>
        /// 主要是 i++这样的
        /// </summary>
        public void MergePostCodes()
        {
            Emit(PostCodes);
            PostCodes.Clear();
        }

        public void Emit(BaseCode baseCode)
        {
            NextCall(baseCode);
            NextCall = (a) => { };
            Codes.Add(baseCode);
            Console.WriteLine("emit " + baseCode.ToMindustryCodeString());
        }
        public void Emit(List<BaseCode> baseCodes)
        {
            if (baseCodes.Count == 0)
                return;
            var first = baseCodes.First();
            NextCall(first);
            NextCall = (a) => { };
            Codes.AddRange(baseCodes);
        }



        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Codes)
            {
                sb.AppendLine(item.ToMindustryCodeString());
            }
            return sb.ToString();
        }
    }

    internal class VariableTable
    {
        int ptr = 0;
        Dictionary<string, LVariable> defines = new();
        Dictionary<ISymbol, LVariable> SymbolDict = new(SymbolEqualityComparer.Default);
        LMethod _method;

        public VariableTable(LMethod method)
        {
            _method = method;
        }

        public LVariable AddLocalVariable(ITypeSymbol type, ISymbol symbol, string name)
        {
            string suffix = "";
            int ptr = 0;
            while (true)
            {
                // process same name
                if (defines.ContainsKey(name + suffix))
                {
                    ptr++;
                    suffix = "_" + ptr;
                    continue;
                }
                var v = new LVariable(_method, name + suffix, type, symbol);
                defines.Add(v.RealName, v);
                SymbolDict.Add(symbol, v);
                return v;
            }
        }

        public LVariable AddTempVariable(ITypeSymbol type)
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

        public LVariable ReNameVariable(LVariable variable)
        {
            // 重命名
            if (variable.Kind != LVariable.VariableType.Temp)
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
                    var v = new LVariable(_method, variable.Name, ++ptr, variable.Type, variable.Symbol, variable.Kind);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    return v;
                }
            }
            else
            {
                // 使用自动生成的名称
                while (true)
                {
                    var v = new LVariable(_method, ++ptr, variable.Type!);
                    if (defines.ContainsKey(v.RealName))
                        continue;
                    defines.Add(v.RealName, v);
                    SymbolDict.Add(v.Symbol!, v);
                    return v;
                }
            }
            throw new Exception();
        }

        public LVariable Get(ISymbol symbol)
        {
            if (TryGet(symbol, out var v))
                return v!;
            throw new Exception("variable not defined");
        }
        public bool TryGet(ISymbol symbol, out LVariable? variable)
        {
            return SymbolDict.TryGetValue(symbol, out variable);
        }
        public ICollection<LVariable> GetAll()
        {
            return defines.Values;
        }

    }
    internal class FieldTable
    {
        Dictionary<string, LVariable> defines = new();
        Dictionary<ISymbol, LVariable> SymbolDict = new(SymbolEqualityComparer.Default);
        LClass _class;

        public FieldTable(LClass @class)
        {
            _class = @class;
        }

        public LVariable Add(ISymbol symbol, ITypeSymbol type, string name)
        {
            var v = new LVariable(_class, type, symbol, name);
            SymbolDict[symbol] = v;
            defines[name] = v;
            return v;
        }
        public bool TryGet(ISymbol symbol, out LVariable? variable)
        {
            return SymbolDict.TryGetValue(symbol, out variable);
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
