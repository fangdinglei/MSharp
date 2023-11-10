
using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile;
using MSharp.Core.Logic;
using MSharp.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program2
{

    static void Read(DirectoryInfo d, List<string> r)
    {
        d.GetDirectories().ToList().ForEach(d => Read(d, r));
        var files = d.GetFiles().ToList().Select(it => File.ReadAllText(it.FullName));
        r.AddRange(files);
    }

    static void Main()
    {
        string code = @"
            using System;
            using TEST1.Core.Logic;

            namespace MyNamespace
            {
                public class TestCPU : GameCPU
                {
                    public Memory cell1=new Memory();
                
                    public override void Main()
                    {
int a=0; self=null;
                        cell1.Write(1,1);
                    }
                }

    public class Memory {
        public double Read(double at) { return 0; }

        public void Write(double value,double at) { }
    }



            }
        ";
        var code2 = @"
            namespace TEST1.Core.Logic
{
    /// <summary>
    /// CPU逻辑
    /// </summary>
    public abstract class GameCPU
    {
        Processor self = new Processor();
        [GameCall(MethodCallMode.Inline)]
        public abstract void Main();
    }


}

";

        var path = CommonUtility.GetCodePathWhenDebug();
        List<string> codes = new List<string>();
        Read(new DirectoryInfo(path + "Core/Logic"), codes);
        Read(new DirectoryInfo(path + "Core/Shared"), codes);
        Read(new DirectoryInfo(path + "Core/Game"), codes);
        Read(new DirectoryInfo(path + "UserCode"), codes);

        new Compiler().Compile(codes.ToArray());
    }

    /// <summary>
    /// 判断是否是CPU逻辑
    /// </summary>
    /// <param name="classSymbol"></param>
    /// <returns></returns>
    static bool IsCPUClass(INamedTypeSymbol classSymbol)
    {
        return classSymbol.BaseType?.Name == nameof(GameCPU);
    }

    /// <summary>
    /// 获取名
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    static string GetName(INamespaceOrTypeSymbol symbol)
    {
        return symbol.Name;
    }
    /// <summary>
    /// 获取全名
    /// </summary>
    /// <param name="symbol"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    static string GetFullName(INamespaceOrTypeSymbol symbol)
    {
#pragma warning disable CS8603 // 可能返回 null 引用。
        return symbol.ToString();
#pragma warning restore CS8603 // 可能返回 null 引用。
        //var res = GetName(symbol);
        //INamespaceOrTypeSymbol? node = symbol.ContainingNamespace as INamespaceOrTypeSymbol;
        //while (node != null)
        //{
        //    res = GetName(node) + "." + res;
        //    node = node.ContainingSymbol as INamespaceOrTypeSymbol;
        //    if (node == null || (node is INamespaceSymbol ns && ns.IsGlobalNamespace))
        //        break;
        //}
        //return res;
    }

    static void AnalyzeCPUCode(CompileContext context, INamedTypeSymbol classSymbol)
    {
        if (!IsCPUClass(classSymbol))
            return;

        string cName = GetName(classSymbol);
        string cFullName = GetFullName(classSymbol);

        LClass lClass = context.CreateClass(classSymbol);
        Dictionary<string, IMethodSymbol> functions = new Dictionary<string, IMethodSymbol>();
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {// 字段
                lClass.Connects.Add(field.Name);
            }
            else if (member is IMethodSymbol method)
            {// 方法
                functions.Add(method.Name, method);
            }
            else
            {
                throw new Exception("不支持的类成员类型" + member);
            }
        }

        IMethodSymbol? mainFunction = functions.GetValueOrDefault(nameof(GameCPU.Main));
        if (mainFunction == null)
            throw new Exception($"没有找到[{cFullName}]的主函数[{nameof(GameCPU.Main)}]，请检查代码是否正确");


        //// 分析方法
        //foreach ((string key, MethodDeclarationSyntax methodDeclaration) in functions)
        //{
        //    AnalyzeFunction(context, classDeclaration, methodDeclaration, lClass);
        //}

        //// 计算可达性 由于裁剪无效代码
        //SortedSet<string> visitedFunctions = new SortedSet<string>();
        //visitedFunctions.Add(nameof(GameCPU));




    }


    ///// <summary>
    ///// 获取类名
    ///// </summary>
    ///// <param name="classDeclaration"></param>
    ///// <returns></returns>
    //static string GetName(ClassDeclarationSyntax classDeclaration)
    //{
    //    return classDeclaration.Identifier.ToString();
    //}
    ///// <summary>
    ///// 获取类全名
    ///// </summary>
    ///// <param name="classDeclaration"></param>
    ///// <returns></returns>
    ///// <exception cref="Exception"></exception>
    //static string GetFullName(ClassDeclarationSyntax classDeclaration)
    //{
    //    var res = GetName(classDeclaration);
    //    SyntaxNode? node = classDeclaration.Parent;
    //    while (node != null)
    //    {
    //        if (node is ClassDeclarationSyntax a)
    //        {
    //            var cName = GetName(a);
    //            res = cName + "." + res;
    //        }
    //        else if (node is NamespaceDeclarationSyntax ns)
    //        {
    //            var nsName = (ns.Name as IdentifierNameSyntax)?.ToString();
    //            res = nsName + "." + res;
    //        }
    //        else if (node is CompilationUnitSyntax)
    //        {
    //            break;
    //        }
    //        node = node.Parent;
    //    }
    //    return res;
    //}

    ///// <summary>
    ///// 判断是否是CPU逻辑
    ///// </summary>
    ///// <param name="classDeclaration"></param>
    ///// <returns></returns>
    //static bool IsCPUClass(ClassDeclarationSyntax classDeclaration)
    //{
    //    if (classDeclaration.BaseList?.Types == null)
    //        return false;
    //    foreach (var type in classDeclaration.BaseList.Types)
    //    {
    //        if (type.Type is IdentifierNameSyntax name && name.Identifier.ToString() == nameof(GameCPU))
    //        {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    ///// <summary>
    ///// 分析CPU逻辑
    ///// </summary>
    ///// <param name="classDeclaration"></param>
    ///// <exception cref="Exception"></exception>
    //static void AnalyzeCPUCode(AnalyzeContext context, ClassDeclarationSyntax classDeclaration)
    //{
    //    if (!IsCPUClass(classDeclaration))
    //        return;



    //    string cName = GetName(classDeclaration);
    //    string cFullName = GetFullName(classDeclaration);

    //    LClass lClass = new LClass(cName, cFullName);
    //    var declarations = classDeclaration.Members;

    //    Dictionary<string, MethodDeclarationSyntax> functions = new Dictionary<string, MethodDeclarationSyntax>();

    //    foreach (var declaration in declarations)
    //    {
    //        if (declaration is FieldDeclarationSyntax field)
    //        {
    //            // 是字段 记录其为连接的建筑物（记录一下，暂时没有实际作用）
    //            lClass.Connects.Add(field.Declaration.Variables[0].Identifier.ToString());
    //        }
    //        else if (declaration is MethodDeclarationSyntax method)
    //        {
    //            // 是函数 先记录一下 等最后从主函数开始编译
    //            functions.Add(method.Identifier.ToString(), method);
    //        }
    //        else
    //        {
    //            Console.WriteLine("暂时不支持" + declaration);
    //        }
    //    }

    //    MethodDeclarationSyntax? mainFunction = functions.GetValueOrDefault(nameof(GameCPU.Main));
    //    if (mainFunction == null)
    //        throw new Exception("没有找到主函数，请检查代码是否正确");

    //    // 分析方法
    //    foreach ((string key, MethodDeclarationSyntax methodDeclaration) in functions)
    //    {
    //        AnalyzeFunction(context, classDeclaration, methodDeclaration, lClass);
    //    }

    //    // 计算可达性 由于裁剪无效代码
    //    SortedSet<string> visitedFunctions = new SortedSet<string>();
    //    visitedFunctions.Add(nameof(GameCPU));




    //}

    //static Block AnalyzeFunction(AnalyzeContext context, ClassDeclarationSyntax classD, MethodDeclarationSyntax methodD, LClass classContext)
    //{
    //    Block res = new Block(classContext);
    //    //foreach (ExpressionStatementSyntax statement in methodD.Body!.Statements)
    //    //{
    //    //    if (statement.Expression is InvocationExpressionSyntax ies) {
    //    //        AnalyzeCall(res, ies);
    //    //    }

    //    //    Console.WriteLine(statement.Expression);
    //    //}


    //    return res;
    //}

    //static void AnalyzeCall(Block method, InvocationExpressionSyntax ies)
    //{
    //    if (ies.Expression is MemberAccessExpressionSyntax maes
    //        && maes.Expression is IdentifierNameSyntax ins)
    //    {
    //        var objectName = ins.Identifier.ToString();
    //        var methodName = maes.Name.Identifier.ToString();
    //        if (method.VarTable.ContainsKey(objectName))
    //        { // 变量表中有该变量，可能是赋值过来的一个游戏对象
    //            //cell1.Write(1, 1);
    //        }
    //        else if (method.Parent.Connects.Contains(objectName))
    //        {
    //            // 是连接到的游戏对象

    //        }
    //        else
    //        {

    //        }
    //    }
    //}
}
