
using Microsoft.CodeAnalysis;
using MSharp.Core.Compile;
using MSharp.Core.Compile.MindustryCode;
using MSharp.Core.Compile.Optimize;
using MSharp.Core.Simulate;
using MSharp.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class App
{

    static void Read(DirectoryInfo d, List<CodeFile> r)
    {
        d.GetDirectories().ToList().ForEach(d => Read(d, r));
        var files = d.GetFiles().Select(it => new CodeFile(it.Name, File.ReadAllText(it.FullName)));
        r.AddRange(files);
    }

    static void Main()
    {
        var path = CommonUtility.GetCodePathWhenDebug();
        List<CodeFile> codes = new List<CodeFile>();
        Read(new DirectoryInfo(path + "Core/Logic"), codes);
        Read(new DirectoryInfo(path + "Core/Shared"), codes);
        Read(new DirectoryInfo(path + "Core/Game"), codes);
        Read(new DirectoryInfo(path + "UserCode"), codes);

        var intermediateCodes = new Compiler().Compile(codes.ToArray());
        Console.WriteLine("=========================================");
        Console.WriteLine(new Compiler().CompileToText(intermediateCodes));
        Console.WriteLine();
        Console.WriteLine();
        MVM mvm = new MVM();
        mvm.Run(intermediateCodes);

        Console.WriteLine("=========================================");
        OptimizerManager optimizerManager = new OptimizerManager();
        intermediateCodes = (List<BaseCode>)optimizerManager.Optimize(intermediateCodes);
        new Compiler().ReIndex(intermediateCodes);
        Console.WriteLine(new Compiler().CompileToText(intermediateCodes));
        mvm = new MVM();
        mvm.Run(intermediateCodes);
    }
}
