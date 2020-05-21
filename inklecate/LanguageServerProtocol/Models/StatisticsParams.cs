using MediatR;
using System;

namespace Ink.LanguageServerProtocol.Models
{
    public class StatisticsParams : IRequest
    {
        public Uri WorkspaceUri { get; set; }
        public Uri MainDocumentUri { get; set; }
        public Ink.Stats Statistics { get; set; }
    }
}
