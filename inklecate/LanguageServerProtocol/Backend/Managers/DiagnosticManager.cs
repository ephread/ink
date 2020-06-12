using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol.Backend
{
    // Compile or run static analysis, then push diagnostics back to the client.
    public class DiagnosticManager: IDiagnosticManager
    {
        private readonly ILogger<DiagnosticManager> _logger;
        private readonly IDiagnosticianFactory _diagnosticianFactory;

        private IDiagnostician _currentDiagnostician;
        private List<Uri> previousFilesWithErrors;

        public DiagnosticManager(
            ILogger<DiagnosticManager> logger,
            IDiagnosticianFactory diagnosticianFactory)
        {
            _logger = logger;

            _diagnosticianFactory = diagnosticianFactory;
        }

        public async Task Compile(Uri documentUri)
        {
            _currentDiagnostician = _diagnosticianFactory.CreateDiagnostician(documentUri);
            previousFilesWithErrors = await _currentDiagnostician.CompileAndDiagnose(previousFilesWithErrors);
        }
    }
}
