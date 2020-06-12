using System;
using System.Threading.Tasks;

using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDiagnosticManager
    {
        Task Compile(Uri scopeUri);
    }
}
