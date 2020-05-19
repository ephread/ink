using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.LanguageServerProtocol.Helpers
{
    public class UriHelper
    {
        public static Uri fromClientUri(Uri uri)
        {
            if (uri.Segments.Length > 1)
            {
                // On windows of the Uri contains %3a local path
                // doesn't come out as a proper windows path
                if (uri.Segments[1].IndexOf("%3a", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return fromClientUri(new Uri(uri.AbsoluteUri.Replace("%3a", ":").Replace("%3A", ":")));
                }
            }
            return uri;
        }

        public static Uri toClientUri(Uri uri)
        {
            var path = uri.LocalPath.Replace(":", "%3A");
            if (!path.StartsWith("/")) return new Uri($"file:///{path}");
            return new Uri($"file://{path}");
        }
    }
}
