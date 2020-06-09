using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface ISymbolStore
    {
        Ink.Parsed.Object SymbolAt(Position position, Uri file);

        void SetSyntaxTree(Ink.Parsed.Object rootObject);
        void SetSymbols(Ink.Stats.Symbols symbols);
    }
}
