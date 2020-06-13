# Ink Language Server

A minimal implementation of the [Language Server Protocol (LSP)] for ink.

[Language Server Protocol (LSP)]: https://microsoft.github.io/language-server-protocol/specification

⚠️ It's under heavy development.

## Features
- [x] Diagnostics
- [x] Statistics (through extensions)
- [x] Go to definition & Hovers
- [ ] Multi-root support
- [ ] Story Previews
- [ ] Auto completion
- [ ] Refactoring

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

## Contributing

### Debugging

To test the server, you can take advantage of the [Visual Studio Code extension](https://github.com/ephread/vscode-ink).

Once an ink file is opened in VS Code, the extension will start the language server and a debugger can then be attached to the `inklecate` process.

### Architectural Overwiew

#### Introduction
The language server is built on top of [Omnisharp's base implementation] and thus tries to follows its patterns. The base implementation relies heavily on dependency injection and interface-based programming.

[Omnisharp's base implementation]: https://github.com/OmniSharp/csharp-language-server-protocol

`LanguageServerHost` is the entry point. Its role is to instantiate the language server, register the request handlers against it and build the dependency container.

Request handlers are registered through their types. The server will instanciate them when required and resolve their dependencies automatically. The most important handler is `InkTextDocumentHandler`, which respond to [text document synchronisation] requests.

[text document synchronisation]: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#textDocument_synchronization

#### Virtual Workspace & File Handlers

`VirtualWorkspaceManager` is a singleton storing the current state of the workspace. It's tracking and storing opened buffers (which are likely different from the disk versions) as well as compilation results for further use.

It works in tandem with `WorkspaceFileHandler`, which is in charge of loading and providing the content of the files present in the workspace. `WorkspaceFileHandler`s abstract away whether the file need to be loaded from the disk, or from the virtual workspace. `WorkspaceFileHandler`s are also transients and define a resolution context: they depend on the current document making the request. In other words, a context defines how to resolve the relative paths of the native `INCLUDE` directive.

A _main document_ is defined as the ink file provided as input to the compiler.

In workspaces without a main document, files are compiled in isolation. Their resolution context is tied to their own location, so they are their own _main document_. But in workspaces defining a _main document_, that document is used and becomes the resolution context.

#### Compilation & Diagnostics

When the client opens a document or reports that a document was changed, `InkTextDocumentHandler` asks `DiagnosticManager` to perform a compilation and push any errors it found to the client.

`DiagnosticManager` then creates a `Diagnostician`. `Diagnostician`s parse/compile the project, generate statistics and store the result in the workpsace for further use. Any errors reported by the compiler are tracked and transformed into Diagnostics which can be pushed to the client.

#### Definitions & Hovers

When the client request a definition, `InkDefinitionHandler` asks `DefinitionManager` to find the token currently under the cursor and return the location of the definition if applicable. Hover requests work in a similar way, using `InkHoverHandler`.

`DefinitionManager` then either creates a `DefinitionFinder` if it doesn't exist or retrieve the existing one for the current document. `DefinitionFinder`s look through the compilation result to find which token is currenlty under the cursor and resolve it definition location.

### Desirable Features

#### Multi-root workspaces

Supporting [multi-root workspace] would be a nice addition. It's probably the less self-contained change as it may require touching most components.

[Multi-root workspace]: https://microsoft.github.io/language-server-protocol/specifications/specification-3-14/#workspace_workspaceFolders

#### Code Completion

A much needed feature. It shouldn't be too hard to build a list of suggestions from the compilation results. Three tricky parts though:

1. making the completion available while a project can't build. Maybe keep track of the last successful result?
2. figuring out the context on a project that doesn't build, it would probably need to be done with a mix of traversing the tree the compiler was able to build partially and a few regular expressions against the current line.
3. performance issues, how fast can the server build the project and provide completion as the user types?
4. the API for [`textDocument/completion`].

[`textDocument/completion`]: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#textDocument_completion

#### Story Preview

Including previews should be straightforward on the server side should be straightforward. `CommandLinePlayer.cs` gives the general idea.

Extensions can be used to communicate between the client and the server. The language server protocol allows the client to execute commands on the server through [`workspace/executeCommand`]. Hence, the client could request a preview by sending the `previewStory` command. Then, the server would notify the client through the following request:

- `preview/text` – the client should display the content as text;
- `preview/tag` – the client should display the content as tags;
- `preview/choice` – the client should display the content as a choice option;
- `preview/prompt` – the client should ask the user to select previoulsy displayed options;
- `preview/endOfStory` – the client should indicate that the story ended;
- `preview/error` – the client should prominetly display the runtime error.

After receiving a `preview/choice` request, the client could send back the choice through a command named `chooseChoice` or `selectOption`. The wording is not intended to be final.

It's unknown at this point whether Omnisharp's based implementation supports custom requests.

[`workspace/executeCommand`]: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#workspace_executeCommand

#### Refactoring

Refactoring shouldn't be too difficult to implement, but any issues with the implementation might wreck havoc on projects. Things to consider:

1. refactoring should only be allowed in a clean state, that is after a successful compilation and before another compilation is attempted (see [`textDocument/prepareRename`]);
2. while we currently keep track of declarations for knots, stitches, etc., we don't keep track of references.

About #2: It would take too long to find all the references after each compilation. Luckily, [`textDocument/rename`] isn't expected to be instantaneous and supports sending progress updates, so a fresh traversal could be performed for each request.

[`textDocument/prepareRename`]: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#textDocument_prepareRename
[`textDocument/rename`]: https://microsoft.github.io/language-server-protocol/specifications/specification-current/#textDocument_rename