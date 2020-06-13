using MediatR;
using System;

namespace Ink.LanguageServerProtocol.Models
{
    /// <summary>
    /// Parameters sent to the client as a response of the
    /// "story/statistics" request.
    /// </summary>
    public class StatisticsParams : IRequest
    {
        public Uri WorkspaceUri { get; set; }
        public Uri MainDocumentUri { get; set; }
        public Stats Statistics { get; set; }
    }
}
