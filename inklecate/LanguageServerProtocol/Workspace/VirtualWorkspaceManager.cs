using System;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Workspace
{
    // This class stores buffers for opened documents. In the
    // future it will also return the content of any files in the workspace
    // regardless of whether it's stored on the disk or in memory.
    public class VirtualWorkspaceManager: ReadOnlyVirtualWorkspaceManager, IVirtualWorkspaceManager
    {

/* ************************************************************************** */

        public VirtualWorkspaceManager(
            ILogger<VirtualWorkspaceManager> logger,
            ILanguageServerEnvironment environment): base(logger, environment)
        {

        }

/* ************************************************************************** */

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

        public IReadOnlyVirtualWorkspaceManager ReadOnlyCopy() {
            return new ReadOnlyVirtualWorkspaceManager(this);
        }
    }
}
