using System;

namespace MSharp.Core.Shared
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class GameApiAttribute : Attribute
    {
        public string Name;
        /// <summary>
        /// The total number of command parameters for this type of command. Some game APIs require many parameters, but most APIs do not require that many parameters
        /// </summary>
        public int ParameterCount;
        /// <summary>
        /// if no default target,target needed
        /// </summary>
        public bool NeedTarget;
        public GameApiAttribute(string name, int parameterCount, bool needTarget = false)
        {
            Name = name;
            ParameterCount = parameterCount;
            NeedTarget = needTarget;
        }
    }

}
