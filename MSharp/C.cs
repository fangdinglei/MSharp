
using Microsoft.CodeAnalysis;
using MSharp.Core.CodeAnalysis.Compile;
using MSharp.Core.Utility;
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
        var path = CommonUtility.GetCodePathWhenDebug();
        List<string> codes = new List<string>();
        Read(new DirectoryInfo(path + "Core/Logic"), codes);
        Read(new DirectoryInfo(path + "Core/Shared"), codes);
        Read(new DirectoryInfo(path + "Core/Game"), codes);
        Read(new DirectoryInfo(path + "UserCode"), codes);

        new Compiler().Compile(codes.ToArray());
    }
}
