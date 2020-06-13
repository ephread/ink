using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Ink.LanguageServerProtocol.Extensions;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.JsonRpc;

namespace Ink.LanguageServerProtocol.Handlers
{
    /// <summary>
    /// Handle 'textDocument/definition' request.
    /// </summary>
    public class InkDefinitionHandler: DefinitionHandler
    {
        private readonly ILogger<InkDefinitionHandler> _logger;
        private readonly IDefinitionManager _definitionManager;

        private static readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.ink"
            }
        );

        public InkDefinitionHandler(
            ILogger<InkDefinitionHandler> logger,
            IDefinitionManager definitionManager)
            : base(new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            })
        {
            _logger = logger;
            _definitionManager = definitionManager;
        }

        public async override Task<LocationOrLocationLinks> Handle(
            DefinitionParams request,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Received 'textDocument/definition' for: '{request.TextDocument.Uri}'");

            LocationOrLocationLinks locations;
            using (_logger.TimeDebug("Definition Search"))
            {
                locations = await _definitionManager.GetDefinition(
                    request.Position,
                    request.TextDocument.Uri,
                    cancellationToken);
            }

            return locations;
        }

        public override void SetCapability(DefinitionCapability capability)
        {

        }
    }
}
