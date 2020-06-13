using System;
using Ink.LanguageServerProtocol.Models;

namespace Ink.LanguageServerProtocol.Workspace.Interfaces
{
    /// <summary>
    /// The configuration settings are not know at the time of the services
    /// registration, since they are sent by the client during the
    /// initialisation request. Yet, we need to make these properties
    /// available for injection at the time of registration. This Singleton
    /// can be injected without waiting for the request to come,
    /// as its properties can be updated at a later stage.
    /// </summary>
    public interface ILanguageServerEnvironment
    {
        Uri RootUri { get; }
    }
}
