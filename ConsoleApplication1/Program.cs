using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string text="大阿朵司法所地方";
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if ((int)text[i] > 32 && (int)text[i] < 127)
                {
                    result += text[i].ToString();
                }
                else
                {
                    result += string.Format("\\u{0:x4}", (int)text[i]);
                }
            }

            Console.WriteLine(result);
            //Console.ReadKey();
           

            string tmpResult= new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled).Replace(
                 result, x => string.Empty + Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)));

            Console.WriteLine(tmpResult);
            Console.ReadKey();
        }
    }
}
