using System;
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
            _logger.LogDebug($"(WORKSPACE) Retrieving document at key: '{uri}'");

            TextDocumentItem documentItem = null;
            _dictionary.TryGetValue(uri, out documentItem);

            return documentItem;
        }

        public void SetTextDocument(Uri uri, TextDocumentItem document)
        {
            _logger.LogDebug($"(WORKSPACE) Setting document at key: '{uri}'");
            _dictionary[uri] = document;
        }

        public void UpdateContentOfTextDocument(Uri uri, String text)
        {
            TextDocumentItem documentItem = null;
            _dictionary.TryGetValue(uri, out documentItem);

            if (documentItem != null)
            {
                _logger.LogDebug($"(WORKSPACE) Updating document at key: '{uri}'");
                documentItem.Text = text;
            }
            else
            {
                _logger.LogWarning($"(WORKSPACE) Can't update content of TextDocument, nothing found for key: '{uri}'");
            }
        }

        public void RemoveTextDocument(Uri uri)
        {
            _logger.LogDebug($"(WORKSPACE) Removing document at key: '{uri}'");
            _dictionary.Remove(uri);
        }

        public Uri GetUriFromAbsolutePath(string path)
        {
            // Restoring the scheme. It's probably unecessary since file://
            // is appended by default, but it's better not to make assumptions
            // about URIs set by the client.
            // (Also, this URIs. needs to match the key in the virtual
            // workspace storage.)

            var fileUriBuilder = new UriBuilder(path);
            fileUriBuilder.Scheme = _environment.RootUri.Scheme;
            var uri = fileUriBuilder.Uri;

            _logger.LogDebug($"(WORKSPACE) Created Uri: '{uri}' from absolute path: '{path}'");

            return uri;
        }

        public Uri GetUriFromRelativePath(Uri rootUri, string path)
        {
            Uri directory = new Uri(rootUri, ".");
            var uri = new Uri(directory, path);

            _logger.LogDebug($"(WORKSPACE) Created Uri: '{uri}' from root Uri: '{rootUri}' and relative path: '{path}'");

            return uri;
        }
    }
}
