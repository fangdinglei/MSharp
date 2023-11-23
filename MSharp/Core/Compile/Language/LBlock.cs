using MSharp.Core.Compile.MindustryCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MSharp.Core.Compile.Language
{
    internal class LBlock
    {
        public readonly LMethod Method;
        /// <summary>
        /// 代码
        /// </summary>
        public List<BaseCode> Codes = new List<BaseCode>();

        public Action<BaseCode> ReturnCall = (a) => { };

        public Action<BaseCode> ContinueCall = (a) => { };

        public Action<BaseCode> NextCall = (a) => { };

        /// <summary>
        /// 主要是 i++这样的
        /// </summary>
        public List<BaseCode> PostCodes = new List<BaseCode>();

        public LBlock(LMethod method)
        {
            Method = method;
        }

        /// <summary>
        /// 主要是 i++这样的
        /// </summary>
        public void MergePostCodes()
        {
            Emit(PostCodes);
            PostCodes.Clear();
        }

        public void Emit(BaseCode baseCode)
        {
            NextCall(baseCode);
            NextCall = (a) => { };
            Codes.Add(baseCode);
            Console.WriteLine("emit " + baseCode.ToMindustryCodeString());
        }
        public void Emit(List<BaseCode> baseCodes)
        {
            if (baseCodes.Count == 0)
                return;
            var first = baseCodes.First();
            NextCall(first);
            NextCall = (a) => { };
            Codes.AddRange(baseCodes);
        }



        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in Codes)
            {
                sb.AppendLine(item.ToMindustryCodeString());
            }
            return sb.ToString();
        }
    }
}
