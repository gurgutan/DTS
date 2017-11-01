using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DTS.DAL.DataModel
{
    public static class StringHelper
    {
        public static string FormatLinesByScopes(string s)
        {
            s = Regex.Replace(s, @"([^\n])(\{)", "${1}\n${2}");
            s = Regex.Replace(s, @"(\{)([^\n])", "${1}\n${2}");
            s = Regex.Replace(s, @"([^\n])(\})", "${1}\n${2}");
            s = Regex.Replace(s, @"(\})([^\n\,])", "${1}\n${2}");
            s = Regex.Replace(s, @"(,)([^\n])", "${1}\n${2}");
            string[] lines = Regex.Replace(s, @"[ ]+", " ").Split('\n');
            int s_counter = 0;
            string spaces = "";
            for (int i = 0; i < lines.Length; i++)
            {
                s_counter += lines[i].Count(c => c == '{') - lines[i].Count(c => c == '}');
                if (lines[i].FirstOrDefault() == '{')
                    spaces = string.Format("{0," + (s_counter - 1) * 4 + "}", "");
                else
                    spaces = string.Format("{0," + s_counter * 4 + "}", "");
                lines[i] = spaces + lines[i];
            }
            return lines.Aggregate("", (c, n) => c + "\n" + n);
        }
    }
}
