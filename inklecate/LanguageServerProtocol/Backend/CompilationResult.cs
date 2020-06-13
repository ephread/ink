using System;
using System.Collections.Generic;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Models;

namespace Ink.LanguageServerProtocol.Backend
{
    public class CompilationResult: ICompilationResult
    {
        public Parsed.Story Story {get; set;}
        public Stats Stats {get; set;}
        public Dictionary<Uri, List<CompilationError>> Errors {get; set;}
    }
}