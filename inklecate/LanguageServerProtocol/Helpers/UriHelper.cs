using System;
using System.Collections.Generic;
using System.Text;

namespace Ink.LanguageServerProtocol.Helpers
{
    public class UriHelper
    {
        // Heavily inspired by code from omnisharp-roslyn:
        // https://github.com/OmniSharp/omnisharp-roslyn
        //
        // Licensed under the terms of the MIT license.

        // On Windows, VS Code sends URIs where colons are percent encoded.
        // They don't resolve well for some reason, so if there's a percent
        // encoded colon in the URI next to a drive letter (second segment),
        // we just replace it by a regular colon.
        public static Uri fromClientUri(Uri uri)
        {
            if (uri.Segments.Length > 1)
            {
                if (uri.Segments[1].IndexOf("%3a", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return fromClientUri(new Uri(uri.AbsoluteUri.Replace("%3a", ":").Replace("%3A", ":")));
                }
            }
            return uri;
        }

        // Reversing the above transformation.
        // Note: It would be a good idea to enable/disable this feature
        // with a flag. Some Language Client might not like the percent encoded
        // colon.
        public static Uri toClientUri(Uri uri)
        {
            var path = uri.LocalPath.Replace(":", "%3A");
            return fromPath(path);
        }

        public static Uri fromPath(string path)
        {
            if (!path.StartsWith("/")) return new Uri($"file:///{path}");
            return new Uri($"file://{path}");
        }
    }
}
