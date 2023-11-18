using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSharp.Core.Shared
{
    [GameIgnore]
    public class MBool
    {
        public static explicit operator MBool(double v)
        {
            return new MBool();
        }
        public static explicit operator bool(MBool v)
        {
            return true;
        }
        public static explicit operator MBool(bool v)
        {
            return new MBool();
        }
    }
    [GameIgnore]
    internal class GameDataStruct
    {
        public GameDataStruct()
        {
            dynamic a = 1;
            if (a==2)
            {

            }
        }
    }
}
