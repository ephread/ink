using System;

namespace Ink.LanguageServerProtocol.Backend.Interfaces
{
    public interface IDiagnosticianFactory
    {
        IDiagnostician CreateDiagnostician(Uri scopeUri);
    }
}
