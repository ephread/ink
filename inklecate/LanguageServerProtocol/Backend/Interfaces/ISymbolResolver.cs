using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface ISymbolResolver
    {
        Ink.Parsed.Story Story { get; set; }
        object SymbolAt(Position position, Uri file);
    }
}
