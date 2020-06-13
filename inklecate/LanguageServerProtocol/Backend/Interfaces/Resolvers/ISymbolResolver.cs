using System;
using System.Threading;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface ISymbolResolver
    {
        Parsed.Story Story { get; set; }

        object SymbolAt(Position position, Uri file, CancellationToken cancellationToken);
    }
}
