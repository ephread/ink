using System;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;


using OmniSharp.Extensions.JsonRpc;

namespace Ink.LanguageServerProtocol.Handlers
{
    public class InkDefinitionHandler : DefinitionHandler
    {
        private readonly IDiagnosticManager _diagnosticManager;

        private static readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.ink"
            }
        );

        public InkDefinitionHandler(IDiagnosticManager diagnosticManager)
            : base(new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            })
        {
            _diagnosticManager = diagnosticManager;
        }

        public async override Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            return await _diagnosticManager.GetDefinition(request.Position, request.TextDocument.Uri);
        }

        public override void SetCapability(DefinitionCapability capability)
        {

        }
    }
}
