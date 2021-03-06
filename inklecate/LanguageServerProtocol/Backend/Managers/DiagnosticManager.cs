using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ink.LanguageServerProtocol.Backend
{
    public class DiagnosticManager: IDiagnosticManager
    {
        private readonly ILogger<DiagnosticManager> _logger;
        private readonly IDiagnosticianFactory _diagnosticianFactory;

        private IDiagnostician _currentDiagnostician;

        public DiagnosticManager(
            ILogger<DiagnosticManager> logger,
            IDiagnosticianFactory diagnosticianFactory)
        {
            _logger = logger;
            _diagnosticianFactory = diagnosticianFactory;
        }

        public async Task CompileAndDiagnose(Uri documentUri, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Creating new Diagnostician for: '{documentUri}'");
            _currentDiagnostician = _diagnosticianFactory.CreateDiagnostician(documentUri);
            await _currentDiagnostician.CompileAndDiagnose(cancellationToken);
        }
    }
}
