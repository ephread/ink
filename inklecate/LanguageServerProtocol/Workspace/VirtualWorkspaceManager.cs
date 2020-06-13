using System;
using System.IO;
using System.Collections.Generic;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Ink.LanguageServerProtocol.Helpers;
using Ink.LanguageServerProtocol.Backend.Interfaces;

namespace Ink.LanguageServerProtocol.Workspace
{
    public class VirtualWorkspaceManager: IVirtualWorkspaceManager
    {
        private readonly ILogger<VirtualWorkspaceManager> _logger;
        private readonly ILanguageServerEnvironment _environment;

        private readonly Dictionary<Uri, TextDocumentItem> _documents;
        private readonly Dictionary<Uri, ICompilationResult> _compilationResults;

        public Uri Uri {
            get { return _environment.RootUri; }
        }

    /* ********************************************************************** */

        public VirtualWorkspaceManager(
            ILogger<VirtualWorkspaceManager> logger,
            ILanguageServerEnvironment environment)
        {
            _logger = logger;
            _environment = environment;

            _documents = new Dictionary<Uri, TextDocumentItem>();
            _compilationResults = new Dictionary<Uri, ICompilationResult>();
        }

    /* ********************************************************************** */

        public TextDocumentItem GetTextDocument(Uri uri)
        {
            uri = UriHelper.FromClientUri(uri);
            _logger.LogDebug($"Retrieving document at key: '{uri}'");

            _documents.TryGetValue(uri, out TextDocumentItem documentItem);

            return documentItem;
        }

        public void SetTextDocument(Uri uri, TextDocumentItem document)
        {
            uri = UriHelper.FromClientUri(uri);

            _logger.LogDebug($"Setting document at key: '{uri}'");
            _documents[uri] = document;
        }

        public void UpdateContentOfTextDocument(Uri uri, String text)
        {
            uri = UriHelper.FromClientUri(uri);

            _documents.TryGetValue(uri, out TextDocumentItem documentItem);

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
            uri = UriHelper.FromClientUri(uri);

            _logger.LogDebug($"Removing document at key: '{uri}'");
            _documents.Remove(uri);

            // If no documents are opened, compilation results are cleared.
            // This ensures any compilation results attached to the main
            // document is removed.
            if (_documents.Count == 0)
            {
                _compilationResults.Clear();
            }
            // Otherwise, we just remove the compilation result attached to the
            // closed document. Note that if the workspace has a main document,
            // this won't do anything.
            else if (_compilationResults.ContainsKey(uri))
            {
                _compilationResults.Remove(uri);
            }
        }

        public ICompilationResult GetCompilationResult(Uri uri)
        {
            return _compilationResults.GetValueOrDefault(uri);
        }

        public void SetCompilationResult(Uri uri, ICompilationResult result)
        {
            _compilationResults[uri] = result;
        }

        public Uri ResolvePath(string path)
        {
            return ResolvePath(path, null);
        }

        public Uri ResolvePath(string path, Uri mainDirectoryUri)
        {
            Uri uri;

            // Path is already absolute, so it just gets converted to a Uri.
            if (Path.IsPathRooted(path))
            {
                uri = UriHelper.FromPath(path);
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
                uri = UriHelper.FromPath(fullPath);

                _logger.LogDebug($"Created Uri: '{uri}' from directory Uri: '{rootUri}' and relative path: '{path}'");
            }

            return uri;
        }
    }
}
