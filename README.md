# Color Lines

A Windows-first WPF implementation of Color Lines with a cozy board theme and round cat-head pieces.

## Current Status

Phase 1 builds the solution skeleton and the tested core game rules. The WPF app is a minimal shell that proves the desktop project can reference and display core state.

## Requirements

- Windows
- .NET SDK 8

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

## Design

The product design is documented in `docs/superpowers/specs/2026-05-17-color-lines-wpf-design.md`.

## Theme Resources

The first theme is `CozyBoard`. Resource placeholders live under `src/ColorLines.Windows/Assets/Themes/CozyBoard/` with `board`, `pieces`, `effects`, and `sounds` folders.
