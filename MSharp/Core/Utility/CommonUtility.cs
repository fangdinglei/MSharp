﻿using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MSharp.Core.Utility
{
    static public class CommonUtility
    {
        /// <summary>
        /// debug模式下获取代码路径
        /// </summary>
        /// <returns></returns>
        static public string GetCodePathWhenDebug()
        {
            var path = Directory.GetCurrentDirectory();
            return Regex.Match(path, @".*(?=bin\\Debug)").Value;
        }

        static public bool AddIfNotNull<T>(this List<T> ls, T? value)
        {
            if (value != null)
            {
                ls.Add(value);
                return true;
            }
            return false;
        }
        static public bool AddIfNotNull<T>(this HashSet<T> set, T? value)
        {
            if (value != null)
            {
                set.Add(value);
                return true;
            }
            return false;
        }
    }
}
