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
        private readonly Dictionary<Uri, TextDocumentItem> _dictionary;

        public VirtualWorkspaceManager(ILogger<VirtualWorkspaceManager> logger)
        {
            _logger = logger;
            _dictionary = new Dictionary<Uri, TextDocumentItem>();
        }

        public TextDocumentItem GetTextDocument(Uri uri)
        {
            _logger.LogDebug("[VIRTUAL WORKSPACE] Got document at: " + uri);
            return _dictionary[uri];
        }

        public void SetTextDocument(Uri uri, TextDocumentItem document)
        {
            _dictionary[uri] = document;
            _logger.LogDebug("[VIRTUAL WORKSPACE] Set document at: " + uri);
        }

        public void UpdateContentOfTextDocument(Uri uri, String text)
        {
            var document = _dictionary[uri];
            if (document != null)
            {
                document.Text = text;
            }
            // else throw? create?
            _logger.LogDebug("[VIRTUAL WORKSPACE] Updated document at: " + uri);
        }

        public void RemoveTextDocument(Uri uri)
        {
            _dictionary.Remove(uri);
            _logger.LogDebug("[VIRTUAL WORKSPACE] Removed document at: " + uri);
        }
    }
}
