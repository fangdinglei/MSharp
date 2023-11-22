namespace MSharp.Core.CodeAnalysis
{
    internal class CodeFile
    {
        public string Code;
        public string FileName;

        public CodeFile(string fileName, string code)
        {
            FileName = fileName;
            Code = code;
        }
    }
}
