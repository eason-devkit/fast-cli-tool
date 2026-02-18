# Fast CLI Tool

A Windows desktop app for quickly launching AI CLI tools (e.g. Claude Code) in your project directories.

![screenshot](pic/Snipaste_2025-10-06_20-40-33.png)

## Features

- **Path Management** — Add and organize your project folders
- **Quick Launch** — Open AI CLI tools (e.g. `claude.cmd`) directly in any project path
- **Custom Commands** — Define multiple custom commands per project (runs via `cmd.exe`)
- **Search** — Instantly filter folders by name
- **Settings** — Configure the default CLI command
- **Auto Save** — All changes are persisted automatically

## Tech Stack

- .NET 9.0 / WPF
- MVVM architecture
- Dark theme (VS Code style)

## Build

Requires [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```bash
# Debug build
dotnet build

# Publish as single exe (self-contained)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o bin/publish-single
```

## License

[MIT](LICENSE)
