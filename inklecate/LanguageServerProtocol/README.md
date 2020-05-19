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

## Limitations

The Language Server only supports the `file://` scheme when exchanging URIs.

## Getting started

The server uses standard I/O to communicate with language clients. To start the server from your language client, run:

```shell
$ inklecate -l
```

### Writing a client

#### Configuration Settings
The server supports one configuration setting.

- `ink.languageServer.mainFilePath` is the path to the main ink file, used by `inklecate` to build the story. If the setting is not provided, the current buffer sent by the client will be treated as the main file.

### Logs
The language server is quite chatty, logs are stored in the following directories:

- **Windows:** `C:\Users\<UserName>\AppData\Roaming\inklecate\language_server.txt`
- **Linux:** `/home/<UserName>/.config/inklecate/language_server.txt`
- **macOS:** `/Users/<UserName>/.config/inklecate/language_server.txt`

### Development

To test the server, you can take advantage of the [Visual Studio Code extension](https://github.com/ephread/vscode-ink).

Once an ink file is opened in VS Code, the extension will start the language server and a debugger can then be attached to the `inklecate` process.

## To do
1. Allow cancelling the compilation (with a `CancellationToken`).
2. Run the compilation in a background thread (will require some methods to be thread-safe).
3. Support multi-root workspaces.