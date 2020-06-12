using System;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IHoverResolver
    {
        Hover HoverForSymbolAt(Position position, Uri file, CancellationToken cancellationToken);
    }
}