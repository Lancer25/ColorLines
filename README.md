# Color Lines

Version 0.1.0 is a playable Windows WPF prototype of Color Lines with cat-themed pieces, local save data, menu flow, settings, sound feedback, and move planning hints.

## Features

- Classic Color Lines movement and five-in-a-row clearing rules.
- Windows desktop UI built with WPF.
- Main menu, pause menu, game over flow, settings, and save/load support.
- Cozy Board and 3D Cat Tokens themes with distinct board palettes and cat piece art.
- Difficulty settings that adjust board size.
- Chinese and English UI text.
- Optional path hints, recommended clearing move preview, board pressure status, and reduced animation mode.

## Requirements

- Windows 10 or later.
- .NET SDK 8 for development.
- .NET 8 Desktop Runtime when using the framework-dependent publish output.

## Commands

Build:

```powershell
dotnet build ColorLines.sln
```

Test:

```powershell
dotnet test ColorLines.sln
```

Run Windows app:

```powershell
dotnet run --project src/ColorLines.Windows/ColorLines.Windows.csproj
```

Publish a framework-dependent Windows build:

```powershell
dotnet publish src/ColorLines.Windows/ColorLines.Windows.csproj -c Release -r win-x64 --self-contained false
```

Publish a self-contained Windows build for easier sharing:

```powershell
dotnet publish src/ColorLines.Windows/ColorLines.Windows.csproj -c Release -r win-x64 --self-contained true
```

## Local Data

The Windows app stores local save data at `%LOCALAPPDATA%\ColorLines\save.json`. The file contains high score, sound setting, animation intensity, selected theme id, recent game state, and window size.

## Design Notes

The original product design is documented in `docs/superpowers/specs/2026-05-17-color-lines-wpf-design.md`. Current implementation notes and incremental plans are under `docs/superpowers/`.
