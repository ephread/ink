using System;
using System.Threading.Tasks;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDiagnosticManager
    {
        public Task Compile(Uri scopeUri);
    }
}
