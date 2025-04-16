using System;

namespace MSharp.Core.Shared
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class GameApiAttribute : Attribute
    {
        /// <summary>
        /// command name
        /// </summary>
        public string Name;
        /// <summary>
        /// The total number of command parameters for this type of command. Some game APIs require many parameters, but most APIs do not require that many parameters
        /// </summary>
        public int ParameterCount;
        /// <summary>
        /// if no default target,command target needed(for example, the unit API does not need to specify the charged unit)
        /// </summary>
        public bool NeedTarget;
        /// <summary>
        /// target index in command(most API 0, but memory API is an exception)
        /// </summary>
        public int TargetIndex;

        /// <summary>
        /// Game Api
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameterCount">If the given number of parameters is less than the passed in value, fill in 0 after it</param>
        /// <param name="needTarget"></param>
        public GameApiAttribute(string name, int parameterCount, bool needTarget = false, int targetIndex = 0)
        {
            Name = name;
            ParameterCount = parameterCount;
            NeedTarget = needTarget;
            TargetIndex = targetIndex;
        }
    }

}
