using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDiagnosticManager
    {
        public Task Compile(Uri scopeUri, CancellationToken cancellationToken);
    }
}
