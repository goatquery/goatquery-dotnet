using System;
using System.Linq;

public static class StringExtension
{
    public static bool In(this string str, params string[] strings)
    {
        return strings.Contains(str, StringComparer.OrdinalIgnoreCase);
    }
}