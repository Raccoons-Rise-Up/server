using System.Collections.Generic;
using System.Linq;
using System;
using System.Globalization;

namespace GameServer.Utilities
{
    public static class Utils
    {
        public static List<T> GetEnumList<T>()
        {
            T[] array = (T[])Enum.GetValues(typeof(T));
            List<T> list = new(array);
            return list;
        }
    }
}
