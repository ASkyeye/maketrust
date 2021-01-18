using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace maketrust
{
    public static class Utils
    {
        public static string NormalizePath(this string value )
        {
            return Path.GetFullPath(Environment.ExpandEnvironmentVariables(value));
        }
    }
}
