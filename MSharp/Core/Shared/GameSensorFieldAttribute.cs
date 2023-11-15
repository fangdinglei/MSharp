using System;

namespace MSharp.Core.Shared
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public class GameSensorFieldAttribute : Attribute
    {
        public GameSensorFieldAttribute()
        {
        }
    }

}
