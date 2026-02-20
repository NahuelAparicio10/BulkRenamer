# Bulk Renamer
A Windows desktop application for batch-renaming files of any type of audio, images, text, 3D assets, and more.
Inspired by a Unity Editor tool I made for one of my games, rebuilt as a standalone Windows app with .NET 8 + WPF.

![License](https://img.shields.io/github/license/NahuelAparicio10/BulkRenamer)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![Platform](https://img.shields.io/badge/platform-Windows-blue)


## Download

![Release](https://img.shields.io/github/v/release/NahuelAparicio10/BulkRenamer)
Grab the latest **BulkRenamer.exe** from the [Releases](https://github.com/NahuelAparicio10/BulkRenamer/releases) page.
No installation required — self-contained Windows executable.

---

## Features

- **Folder scope** — select a root folder with optional subfolder traversal
- **Extension filter** — limit files by type e.g. `.mp3, .wav, .ogg`
- **Name filter** — narrow targets by Contains / StartsWith / EndsWith / Exact match
- **Replace modes** — Plain Text or full .NET Regex
- **Apply modes** — Anywhere, Prefix only, Suffix only
- **Case sensitivity** toggle
- **Replace spaces** — replace whitespace with any custom string e.g. `_` or `-`
- **Live preview** — see old → new names before committing, color-coded by status
- **Safety options** — dry-run mode, skip unchanged files, skip name collisions
- **Drag & drop** — drop a folder or files directly onto the window

---
## Architecture

The project follows **MVVM** (Model-View-ViewModel) with a clean separation of concerns:

```
src/BulkRenamer/
├── Core/
│   ├── Models/       — pure data (RenamePreview, RenameSettings)
│   ├── Services/     — business logic (IRenameService, IFileSystemService)
│   └── Enums/        — MatchMode, ApplyMode, ReplaceMode, ScopeMode
├── ViewModels/       — UI state and commands, no WPF dependencies
│   └── Base/         — ViewModelBase, RelayCommand
└── Views/            — XAML only, zero logic in code-behind

tests/
└── BulkRenamer.Tests/
    ├── RenameServiceFilterTests.cs
    ├── RenameServicePlainTextTests.cs
    └── RenameServiceRegexAndCollisionTests.cs
```

**Key design decisions:**
- **Core** has zero WPF dependencies — fully unit-testable in isolation
- **ViewModels** depend only on Core interfaces, never on concrete implementations
- **Views** bind to ViewModels via standard WPF data binding — zero logic in code-behind
- **`readonly record struct`** for `RenamePreview` — avoids heap allocation for large preview lists
- **`EnumerateFiles` (lazy)** over `GetFiles` (eager) — handles large folders without loading all paths into memory upfront

---

## Getting Started

### Requirements
- Windows 10 / 11
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run from source
```bash
git clone https://github.com/NahuelAparicio10/BulkRenamer.git
cd BulkRenamer
dotnet run --project src/BulkRenamer/BulkRenamer.csproj
```

### Run tests
```bash
dotnet test
```

---

## Usage

| Step | Action |
|------|--------|
| 1 | Set the **Folder** path or drag & drop a folder onto the window |
| 2 | Optionally filter by **Extensions** (e.g. `.mp3, .wav`) |
| 3 | Set your **Find** and **Replace With** values |
| 4 | Check the **Preview** panel — green = will rename, red = collision, gray = no change |
| 5 | Disable **Preview Only** and click **Apply Rename** |

### Examples

**Rename audio files — replace prefix:**
- Find: `DIRT -` · Replace With: `SFX_` · Apply To: `Anywhere`
- `DIRT - Run 1.wav` → `SFX_Run 1.wav`

**Remove spaces from all mp3s:**
- Extensions: `.mp3` · Replace Spaces: ✅ · Replace With: `_`
- `My Track 01.mp3` → `My_Track_01.mp3`

**Strip LOD suffix with Regex:**
- Mode: `Regex` · Find: `_LOD\d+$` · Replace With: _(empty)_
- `SM_Weapon_LOD0.fbx` → `SM_Weapon.fbx`

---
