using System;
using System.Collections.Generic;
using System.Text;

namespace KindleLiteratuhr.Common
{
    public static class ModifyExtension
    {
        public static T Modify<T>(this T obj, Func<T, T> action)
        {
            return action(obj);
        }
    }
}
