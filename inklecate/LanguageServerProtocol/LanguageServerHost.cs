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
        /// <summary>
        /// Options used to configure the language server during its creation.
        /// </summary>
        private readonly LanguageServerOptions _options;

        /// <summary>
        /// The Language Server's connection property, wrapped inside a proxy
        /// object.
        /// </summary>
        private readonly LanguageServerConnection _connection;

        /// <summary>
        /// The Language Server's environment property, wrapped inside a proxy
        /// object.
        /// </summary>
        private readonly LanguageServerEnvironment _environment;

        /// <summary>
        /// The language server instance.
        /// </summary>
        private ILanguageServer _server;

    /* ********************************************************************** */

        /// <summary>
        /// Create the Language Server Host.async Not that the actual
        /// language server is creasted by the <c>Start</c> method.
        /// </summary>
        /// <param name="input">Stream used to read data from the client.</param>
        /// <param name="output">Stream used to write data to the client.</param>
        public LanguageServerHost(Stream input, Stream output)
        {
            // Make sure the log won't clutter the current directory by setting
            // its location to AppData (Windows) or ~/.config (Linux & macOS).
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var logFile = Path.Combine(appData, "inklecate", "language_server.txt");

            // Configure Serilog so the logs are both written on the disk and
            // pushed to the client.
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

            // TODO: Configure the log level with a command line option.
        }

    /* ********************************************************************** */

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

    /* ********************************************************************** */

        /// <summary>
        /// Complete initialisation after receiving the request from the client.
        /// </summary>
        /// <param name="server">The fully initialise Language Server.</param>
        /// <param name="initializeParams">
        /// The initialisation parameters sent by the client.
        /// </param>
        /// <returns>A completed task.</returns>
        private Task Initialize(
            ILanguageServer server,
            InitializeParams initializeParams)
        {
            Log.Logger.Debug("Received 'initialize' event.");

            _connection.SetServer(server);
            _environment.SetEnvironment(initializeParams);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Register services used by the dependency injection framework.
        ///
        /// Note that this method ensures that _connection and _environment
        /// are globally available for injection.
        /// </summary>
        /// <param name="services">
        /// The collection in which register new services.
        /// </param>
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

    /* ********************************************************************** */

        private void RegisterManagers(IServiceCollection services)
        {
            services.AddSingleton<IVirtualWorkspaceManager>(provider => {
                var connection = provider.GetService<ILanguageServerConnection>();
                var environment = provider.GetService<ILanguageServerEnvironment>();

                var loggerFactory = provider.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<VirtualWorkspaceManager>();

                return new VirtualWorkspaceManager(logger, environment);
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

                return new WorkspaceFileHandlerFactory(
                    loggerFactory,
                    environment,
                    connection,
                    workspace);
            });

            services.AddTransient<IDiagnosticianFactory>(provider => {
                var connection = provider.GetService<ILanguageServerConnection>();
                var environment = provider.GetService<ILanguageServerEnvironment>();
                var workspace = provider.GetService<IVirtualWorkspaceManager>();

                var fileHandlerFactory = provider.GetService<IWorkspaceFileHandlerFactory>();
                var loggerFactory = provider.GetService<ILoggerFactory>();

                return new DiagnosticianFactory(
                    loggerFactory,
                    fileHandlerFactory,
                    connection,
                    workspace);
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
