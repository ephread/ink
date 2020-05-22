using System;
using Ink.LanguageServerProtocol.Models;

// Ensuring the thread safety of this singleton would be a good idea.
namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    public interface ILanguageServerEnvironment
    {
        Uri RootUri { get; }

        ILanguageServerEnvironment Copy();
    }
}
