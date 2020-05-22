using System;
using System.IO;
using System.Collections.Generic;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace
{
    // This class stores buffers for opened documents. In the
    // future it will also return the content of any files in the workspace
    // regardless of whether it's stored on the disk or in memory.
    public class ReadOnlyVirtualWorkspaceManager: IReadOnlyVirtualWorkspaceManager
    {
        protected readonly ILogger<VirtualWorkspaceManager> _logger;
        protected readonly ILanguageServerEnvironment _environment;

        protected readonly Dictionary<Uri, TextDocumentItem> _dictionary;

/* ************************************************************************** */

        public ReadOnlyVirtualWorkspaceManager(
            ILogger<VirtualWorkspaceManager> logger,
            ILanguageServerEnvironment environment)
        {
            _logger = logger;
            _environment = environment;

            _dictionary = new Dictionary<Uri, TextDocumentItem>();
        }

        public ReadOnlyVirtualWorkspaceManager(ReadOnlyVirtualWorkspaceManager workspace)
        {
            // Logger is not copied, since its logging methods are
            // thread-safe.
            _logger = workspace._logger;
            _environment = workspace._environment.Copy();

            _dictionary = new Dictionary<Uri, TextDocumentItem>();

            foreach (var keyValue in workspace._dictionary) {
                var newKey = new Uri(keyValue.Key.ToString());
                var newValue = new TextDocumentItem() {
                    LanguageId = keyValue.Value.LanguageId,
                    Version = keyValue.Value.Version,
                    Text = keyValue.Value.Text,
                };

                _dictionary[newKey] = newValue;
            }
        }

/* ************************************************************************** */

        public TextDocumentItem GetTextDocument(Uri uri)
        {
            _logger.LogDebug($"Retrieving document at key: '{uri}'");

            TextDocumentItem documentItem = null;
            _dictionary.TryGetValue(uri, out documentItem);

            return documentItem;
        }

        public Uri ResolvePath(string path) {
            return ResolvePath(path, null);
        }

        public Uri ResolvePath(string path, Uri mainDirectoryUri)
        {
            Uri uri;

            // Path is already absolute, so it just gets converted to a Uri.
            if (Path.IsPathRooted(path))
            {
                var builder = new UriBuilder
                {
                    Fragment = null,
                    Host = _environment.RootUri.Host,
                    Port = _environment.RootUri.Port,
                    Query = null,
                    Scheme = _environment.RootUri.Scheme,
                    Path = path,
                };
                uri = builder.Uri;

                _logger.LogDebug($"Created Uri: '{uri}' from absolute path: '{path}'");
            }
            else
            {
                // if mainFileUri was not provided, we're using
                // _environment.RootUri which is always a directory.
                Uri rootUri = mainDirectoryUri ?? _environment.RootUri;

                // Concatenate the paths through Path.Combine()
                // and rebuild the URI. This is necessary since rootUri
                // won't always have a trailing slash.
                // Concatenating two URIs through Uri's constructor strips
                // the last part of the path if the first argument
                // doesn't have a trailing slash.
                var fullPath = Path.Combine(rootUri.LocalPath, path);
                var builder = new UriBuilder
                {
                    Fragment = null,
                    Host = _environment.RootUri.Host,
                    Port = _environment.RootUri.Port,
                    Query = null,
                    Scheme = _environment.RootUri.Scheme,
                    Path = fullPath,
                };
                uri = builder.Uri;

                _logger.LogDebug($"Created Uri: '{uri}' from directory Uri: '{rootUri}' and relative path: '{path}'");
            }

            return uri;
        }
    }
}
