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
    /// Handle 'textDocument/hover' request.
    /// </summary>
    public class InkHoverHandler: HoverHandler
    {
        private readonly ILogger<InkHoverHandler> _logger;
        private readonly IDefinitionManager _definitionManager;

        private static readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.ink"
            }
        );

        public InkHoverHandler(
            ILogger<InkHoverHandler> logger,
            IDefinitionManager definitionManager)
            : base(new TextDocumentRegistrationOptions()
            {
                DocumentSelector = _documentSelector
            })
        {
            _logger = logger;
            _definitionManager = definitionManager;
        }

        public async override Task<Hover> Handle(
            HoverParams request,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Received 'textDocument/hover' for: '{request.TextDocument.Uri}'");

            Hover hover;
            using (_logger.TimeDebug("Hover Search"))
            {
                hover = await _definitionManager.GetHover(
                    request.Position,
                    request.TextDocument.Uri,
                    cancellationToken);
            }

            return hover;
        }
    }
}
