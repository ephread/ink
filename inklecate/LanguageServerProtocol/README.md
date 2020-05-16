# Ink Language Server

A minimal implementation of the [Language Server Protocol (LSP)] for ink.

[Language Server Protocol (LSP)]: https://microsoft.github.io/language-server-protocol/specification

## Features
- [ ] Diagnostics
- [ ] Statistics (through extensions)
- [ ] Story Previews
- [ ] Auto completion
- [ ] Go to definition
- [ ] Refactoring?

## To do
- Investigate whether it's possible to unwrap exceptions from asynchronous calls.

## Contributing

### Logs
The language server is quite chatty, logs are stored in the following directories:

- **Windows:** `C:\Users\<UserName>\AppData\Roaming\inklecate\language_server.txt`
- **Linux:** `/home/<UserName>/.config/inklecate/language_server.txt`
- **macOS:** `/Users/<UserName>/.config/inklecate/language_server.txt`

### How to
- Debug the server
- Run with the VS Code Language Extension Configuration