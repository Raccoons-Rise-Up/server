using System.Collections.Generic;
using System.Linq;

namespace GameServer.Utilities
{
    public static class Utils
    {
        public static string AddSpaceBeforeEachCapital(string str) => string.Concat(str.Select(x => char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
    }
}
