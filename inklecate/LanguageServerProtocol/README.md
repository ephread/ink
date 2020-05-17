# Ink Language Server

A minimal implementation of the [Language Server Protocol (LSP)] for ink.

[Language Server Protocol (LSP)]: https://microsoft.github.io/language-server-protocol/specification

⚠️ It's under heavy development.

## Features
- [x] Diagnostics
- [ ] Statistics (through extensions)
- [ ] Story Previews
- [ ] Auto completion
- [ ] Go to definition
- [ ] Refactoring?

## Getting started

The server use standard I/O to communicate with language clients. To start the server from your language client, run:

```shell
$ inklecate -s
```

### Writing a client

#### Configuration Settings
The server supports one configuration settings.

- `ink.languageServer.mainFilePath` is path to the main ink file, used by inklecate to build the story. If the setting is not provided, the current buffer sent by the client will be treated as the main file.

### Logs
The language server is quite chatty, logs are stored in the following directories:

- **Windows:** `C:\Users\<UserName>\AppData\Roaming\inklecate\language_server.txt`
- **Linux:** `/home/<UserName>/.config/inklecate/language_server.txt`
- **macOS:** `/Users/<UserName>/.config/inklecate/language_server.txt`

### Development

To test the server, you can take advantage of the [Visual Studio Code extension](https://github.com/ephread/vscode-ink).

Once an ink file is opened in VS Code, the extension will start the language server and a debugger can then be attached to the process.

## To do
- Support compilation cancellation (`CancellationToken`).
- Offload the compilation to a background thread (will require some methods to be thread-safe).