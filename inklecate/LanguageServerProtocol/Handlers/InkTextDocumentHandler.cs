using System;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace Ink.LanguageServerProtocol.Handlers
{
    public class InkTextDocumentHandler: ITextDocumentSyncHandler
    {
        private readonly ILogger<InkTextDocumentHandler> _logger;
        private readonly IVirtualWorkspaceManager _virtualWorkspace;

        private readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.ink"
            }
        );

        public InkTextDocumentHandler(ILogger<InkTextDocumentHandler> logger, IVirtualWorkspaceManager workspace)
        {
            _logger = logger;
            _virtualWorkspace = workspace;

            logger.LogDebug("[HANDLER] InkTextDocumentHandler is ready.");
        }

        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
        {
            return new TextDocumentAttributes(uri, "ink");
        }

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[HANDLER] Received 'textDocument/didChange' for: " + request.TextDocument.Uri);

            // Since synchronisation is requested as full text, it's assumed there
            // will be only one change in the collection for now.
            var enumerator = request.ContentChanges.GetEnumerator();
            if (enumerator.MoveNext())
            {
                var change = enumerator.Current;
                _virtualWorkspace.UpdateContentOfTextDocument(request.TextDocument.Uri, change.Text);
            }

            // Then compile / perform analysis.

            return Unit.Task;
        }

        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[HANDLER] Received 'textDocument/didOpen' for: " + request.TextDocument.Uri);
            _virtualWorkspace.SetTextDocument(request.TextDocument.Uri, request.TextDocument);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[HANDLER] Received 'textDocument/didClose' for: " + request.TextDocument.Uri);
            _virtualWorkspace.RemoveTextDocument(request.TextDocument.Uri);

            return Unit.Task;
        }

        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("[HANDLER] Received 'textDocument/didSave' for: " + request.TextDocument.Uri);

            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability)
        {

        }

        TextDocumentChangeRegistrationOptions IRegistration<TextDocumentChangeRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions() {
                DocumentSelector = _documentSelector,
                SyncKind = TextDocumentSyncKind.Full
            };
        }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentRegistrationOptions() {
                DocumentSelector = _documentSelector,
            };
        }

        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
        {
            return new TextDocumentSaveRegistrationOptions() {
                DocumentSelector = _documentSelector,
                IncludeText = true
            };
        }
    }
}