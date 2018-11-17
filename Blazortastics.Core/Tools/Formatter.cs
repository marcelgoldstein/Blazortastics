using System;

namespace Blazortastics.Core.Tools
{
    public static class Formatter
    {
        public static string Format(this TimeSpan span)
        {
            return $"{span.Minutes:D2}m {span.Seconds:D2}s {span.Milliseconds:D3}ms";
        }
    }
}
