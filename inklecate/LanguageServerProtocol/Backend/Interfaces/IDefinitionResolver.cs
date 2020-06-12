using System;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDefinitionResolver
    {
        LocationOrLocationLinks DefinitionForSymbolAt(Position position, Uri file);
    }
}