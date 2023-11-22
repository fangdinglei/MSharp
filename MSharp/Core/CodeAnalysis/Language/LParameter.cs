namespace MSharp.Core.CodeAnalysis.Language
{

    internal class LParameter
    {
        public bool? Used = true;
        public object? DefaultValue { get; init; }
        public LVariable Variable { get; init; }

        public LParameter(LVariable variable, object? defaultValue)
        {
            Variable = variable;
            DefaultValue = defaultValue;
        }
    }
}
