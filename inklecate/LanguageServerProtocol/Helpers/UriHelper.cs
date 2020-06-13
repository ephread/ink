using System;
using System.Collections.Generic;
using System.Text;

//     Heavily inspired by code from omnisharp-roslyn:
//     https://github.com/OmniSharp/omnisharp-roslyn
//
//     Licensed under the terms of the MIT license.
namespace Ink.LanguageServerProtocol.Helpers
{
    /// <summary>
    /// Provide a set of methods to sanitise Uris.
    /// </summary>
    public class UriHelper
    {
        /// <summary>
        /// Turn a "client" Uri into a "server" Uri.
        ///
        /// On Windows, VS Code sends URIs where colons are percent encoded.
        /// They don't resolve well for some reason, so if there's a percent
        /// encoded colon in the URI next to a drive letter (second segment),
        /// we just replace it by a regular colon.
        /// </summary>
        /// <param name="uri">The Uri to convert</param>
        /// <returns>Returns a sanitised "server" Uri</returns>
        public static Uri FromClientUri(Uri uri)
        {
            if (uri.Segments.Length > 1)
            {
                if (uri.Segments[1].IndexOf("%3a", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    return FromClientUri(new Uri(uri.AbsoluteUri.Replace("%3a", ":").Replace("%3A", ":")));
                }
            }
            return uri;
        }

        /// <summary>
        /// Turn a "server" Uri into a "client" Uri, reversing
        /// the transformation performed in FromClientUri.
        ///
        /// Note: It would be a good idea to enable/disable this feature
        /// with a flag. Some Language Client might not like the percent
        /// encoded colon.
        /// </summary>
        /// <param name="uri">The Uri to convert</param>
        /// <returns>Returns a "client" Uri</returns>
        public static Uri ToClientUri(Uri uri)
        {
            var path = uri.LocalPath.Replace(":", "%3A");
            return FromPath(path);
        }

        /// <summary>
        /// Turn a path into a Uri.
        /// </summary>
        /// <param name="path">The path to convert</param>
        /// <returns>A Uri representing the path.</returns>
        public static Uri FromPath(string path)
        {
            if (!path.StartsWith("/")) return new Uri($"file:///{path}");
            return new Uri($"file://{path}");
        }
    }
}
