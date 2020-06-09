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
        private readonly TextDocumentRegistrationOptions _options;
        private readonly IDiagnosticManager _processor;

        public InkDefinitionHandler(TextDocumentRegistrationOptions registrationOptions): base(registrationOptions)
        {

        }

        public async override Task<LocationOrLocationLinks> Handle(DefinitionParams request, CancellationToken cancellationToken)
        {
            return null;
        }

        public override void SetCapability(DefinitionCapability capability)
        {

        }
    }
}
