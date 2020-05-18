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
    public class VirtualWorkspaceManager: IVirtualWorkspaceManager
    {
        private readonly ILogger<VirtualWorkspaceManager> _logger;
        private readonly ILanguageServerEnvironment _environment;
        private readonly ILanguageServerConnection _connection;

        private readonly Dictionary<Uri, TextDocumentItem> _dictionary;

/* ************************************************************************** */

        public VirtualWorkspaceManager(
            ILogger<VirtualWorkspaceManager> logger,
            ILanguageServerEnvironment environment,
            ILanguageServerConnection connection)
        {
            _logger = logger;
            _environment = environment;
            _connection = connection;

            _dictionary = new Dictionary<Uri, TextDocumentItem>();
        }

/* ************************************************************************** */

        public TextDocumentItem GetTextDocument(Uri uri)
        {
            _logger.LogDebug($"Retrieving document at key: '{uri}'");

            TextDocumentItem documentItem = null;
            _dictionary.TryGetValue(uri, out documentItem);

            return documentItem;
        }

        public void SetTextDocument(Uri uri, TextDocumentItem document)
        {
            _logger.LogDebug($"Setting document at key: '{uri}'");
            _dictionary[uri] = document;
        }

        public void UpdateContentOfTextDocument(Uri uri, String text)
        {
            TextDocumentItem documentItem = null;
            _dictionary.TryGetValue(uri, out documentItem);

            if (documentItem != null)
            {
                _logger.LogDebug($"Updating document at key: '{uri}'");
                documentItem.Text = text;
            }
            else
            {
                _logger.LogWarning($"Can't update content of TextDocument, nothing found for key: '{uri}'");
            }
        }

        public void RemoveTextDocument(Uri uri)
        {
            _logger.LogDebug($"Removing document at key: '{uri}'");
            _dictionary.Remove(uri);
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
