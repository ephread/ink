using System.IO;
using System.Threading.Tasks;
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
        private ILanguageServerConnection _connection;
        private ILanguageServerEnvironment _environment;

        public LanguageServerHost(Stream input, Stream output)
        {
            // Configure Serilog. With the current setup,
            // Log.Logger calls will write to the disk.
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(
                    "ink.language.server.log.txt",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Logger.Information("Logger is ready.");

            // Configure the Language Server:
            //
            //   - `WithInput` & `WithOutput`: define from/to
            //     which stream the language server will read/write.
            //     In practice, it's using the Console.
            //
            //   - `ConfigureLogging`: Almost every object in the Language
            //     Server implementation will take a logger as dependency.
            //     At this point, loggers are only used to report valuable
            //     information to the client and don't write to the disk.
            //
            //   - `WithHandler` is expecting a Handler, i. e. an object that
            //     respond to LSP commands (such as 'textDocument/didOpen').
            //     Note that a Type is passed here, as the actual object will
            //     be instanciated by the server. Any dependencies defined
            //     by its constructor will be resolved by the service system
            //     (see WithServices).
            //
            //   - `WithServices` is expecting an Action to provides dependency
            //     injection services.
            //
            //   - `OnInitialize` & `OnInitialized` are expecting delegates;
            //      the server will call them when receiving initialisation
            //      events.
            _options = new LanguageServerOptions()
                .WithInput(input)
                .WithOutput(output)
                .ConfigureLogging(x => x
                    .AddSerilog()
                    .AddLanguageServer()
                    .SetMinimumLevel(LogLevel.Debug))
                .WithHandler<InkTextDocumentHandler>()
                .WithServices(Services)
                .OnInitialize(Initialize)
                .OnInitialized(Initialized);
        }

        // TODO: Wait for initialize event to register services and handlers.
        private void Services(IServiceCollection services)
        {
            _connection = new LanguageServerConnection();
            services.AddSingleton(provider => {
                return _connection;
            });

            services.AddSingleton<IVirtualWorkspaceManager>(provider => {
                var loggerFactory = provider.GetService<ILoggerFactory>();
                var connection = provider.GetService<ILanguageServerConnection>();

                var logger = loggerFactory.CreateLogger<VirtualWorkspaceManager>();

                return new VirtualWorkspaceManager(logger, connection);
            });
        }

        private Task Initialize(
            ILanguageServer server,
            InitializeParams initializeParams)
        {
            Log.Logger.Information("Received 'initialize'.");

            _environment = new LanguageServerEnvironment(initializeParams);

            return Task.CompletedTask;
        }

        private Task Initialized(
            ILanguageServer server,
            InitializeParams initializedParams,
            InitializeResult response)
        {
            Log.Logger.Information("Received 'initialized'.");

            return Task.CompletedTask;
        }

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
    }
}
