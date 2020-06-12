using System;
using System.IO;
using System.Threading.Tasks;
using Ink.LanguageServerProtocol.Backend;
using Ink.LanguageServerProtocol.Backend.Interfaces;
using Ink.LanguageServerProtocol.Handlers;
using Ink.LanguageServerProtocol.Workspace;
using Ink.LanguageServerProtocol.Workspace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Server;
using Serilog;
using ILanguageServer = OmniSharp.Extensions.LanguageServer.Server.ILanguageServer;
using LanguageServer = OmniSharp.Extensions.LanguageServer.Server.LanguageServer;

namespace Ink.LanguageServerProtocol
{
    public class LanguageServerHost
    {
        private readonly LanguageServerOptions _options;
        private ILanguageServer _server;
        private LanguageServerConnection _connection;
        private LanguageServerEnvironment _environment;

/* ************************************************************************** */

        public LanguageServerHost(Stream input, Stream output)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logFile = Path.Combine(appData, "inklecate", "language_server.txt");

            // Configure Serilog so that the logs will both can be written on
            // the disk on top of being pushed to the client.
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logFile,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger.Information("Logger is ready.");

            _connection = new LanguageServerConnection();
            _environment = new LanguageServerEnvironment();

            // Configure the Language Server:
            //
            //   - `WithInput` & `WithOutput`: define from/to
            //     which stream the language server will read/write.
            //     In practice, it's using the Console.
            //
            //   - `ConfigureLogging`: Almost every object in the Language
            //     Server implementation will take a logger as dependency.
            //     Loggers write on the disk (AddSerilog) and also push logs
            //     to the client (AddLanguageServer).
            //
            //   - `WithHandler` is expecting a Handler, i. e. an object that
            //     respond to LSP commands (such as 'textDocument/didOpen').
            //     Note that a Type is passed here, as the actual object will
            //     be instanciated by the server. Any dependencies defined
            //     by its constructor will be resolved by the service system
            //     (see WithServices).
            //
            //   - `WithServices` is expecting an Action to provides dependency
            //     injection services. The host keep a refernce to the service
            //     collection, so that new service can be registered after
            //     the server was initialised.
            //
            //   - `OnInitialize` (& `OnInitialized`) is expecting a delegate
            //      which will be called when receiving initialisation
            //      events.
            _options = new LanguageServerOptions()
                .WithInput(input)
                .WithOutput(output)
                .ConfigureLogging(x => x
                    .AddSerilog()
                    .AddLanguageServer()
                    .SetMinimumLevel(LogLevel.Debug))
                .WithHandler<InkTextDocumentHandler>()
                .WithHandler<InkDefinitionHandler>()
                .WithHandler<InkHoverHandler>()
                .WithServices(Services)
                .OnInitialize(Initialize);
        }

/* ************************************************************************** */

        /// <summary>
        /// Create and start the Language Server asynchronously.
        /// </summary>
        public async Task Start()
        {
            Log.Logger.Information("Starting Language Server…");

            _server = await LanguageServer.From(_options);

            Log.Logger.Information("Language Server is ready.");

            await _server.WaitForExit;
        }

/* ************************************************************************** */

        private Task Initialize(
            ILanguageServer server,
            InitializeParams initializeParams)
        {
            Log.Logger.Debug("Received 'initialize' event.");

            _connection.SetServer(server);
            _environment.SetEnvironment(initializeParams);

            return Task.CompletedTask;
        }

        // Setting up Dependencies.
        private void Services(IServiceCollection services)
        {
            services.AddSingleton<ILanguageServerConnection>(provider => {
                return _connection;
            });

            services.AddSingleton<ILanguageServerEnvironment>(provider => {
                return _environment;
            });

            RegisterFactories(services);
            RegisterManagers(services);
        }

        private void RegisterManagers(IServiceCollection services)
        {
            services.AddSingleton<IVirtualWorkspaceManager>(provider => {
                var connection = provider.GetService<ILanguageServerConnection>();
                var environment = provider.GetService<ILanguageServerEnvironment>();

                var loggerFactory = provider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<VirtualWorkspaceManager>();

                return new VirtualWorkspaceManager(logger, environment, connection);
            });

            services.AddSingleton<IDiagnosticManager>(provider => {
                var diagnosticianFactory = provider.GetService<IDiagnosticianFactory>();

                var loggerFactory = provider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<DiagnosticManager>();

                return new DiagnosticManager(logger, diagnosticianFactory);
            });

            services.AddSingleton<IDefinitionManager>(provider => {
                var definitionFinderFactory = provider.GetService<IDefinitionFinderFactory>();

                var loggerFactory = provider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<DefinitionManager>();

                return new DefinitionManager(logger, definitionFinderFactory);
            });
        }

        private void RegisterFactories(IServiceCollection services)
        {
            services.AddTransient<IWorkspaceFileHandlerFactory>(provider => {
                var connection = provider.GetService<ILanguageServerConnection>();
                var environment = provider.GetService<ILanguageServerEnvironment>();
                var workspace = provider.GetService<IVirtualWorkspaceManager>();

                var loggerFactory = provider.GetService<ILoggerFactory>();

                return new WorkspaceFileHandlerFactory(loggerFactory, environment, connection, workspace);
            });

            services.AddTransient<IDiagnosticianFactory>(provider => {
                var connection = provider.GetService<ILanguageServerConnection>();
                var environment = provider.GetService<ILanguageServerEnvironment>();
                var workspace = provider.GetService<IVirtualWorkspaceManager>();

                var fileHandlerFactory = provider.GetService<IWorkspaceFileHandlerFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();

                return new DiagnosticianFactory(loggerFactory, fileHandlerFactory, environment, connection, workspace);
            });

            services.AddTransient<IDefinitionFinderFactory>(provider => {
                var workspace = provider.GetService<IVirtualWorkspaceManager>();

                var fileHandlerFactory = provider.GetService<IWorkspaceFileHandlerFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();

                return new DefinitionFinderFactory(loggerFactory, fileHandlerFactory, workspace);
            });
        }

    }
}
