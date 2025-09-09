using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BE_PHOITRON.Infrastructure.Shared
{
    public class CheckValidPropertyPath
    {
        public static bool IsValidPropertyPath<T>(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            var type = typeof(T);
            foreach (var part in path.Split('.'))
            {
                var p = type.GetProperty(part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) return false;
                type = p.PropertyType;
            }
            return true;
        }
    }
}
