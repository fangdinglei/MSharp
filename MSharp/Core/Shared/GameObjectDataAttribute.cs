using System;

namespace MSharp.Core.Shared
{
    [AttributeUsage(AttributeTargets.Field|AttributeTargets.Property, Inherited = false)]
    public class GameObjectDataAttribute : Attribute
    {
        public GameObjectDataAttribute()
        {
        }
    }

}
