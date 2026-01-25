# CLAUDE.md - Etupirka Project Guide

## Project Overview

**Etupirka** is a Windows desktop application for tracking play-time of visual novels (eroge). It monitors game processes, tracks focus time, and integrates with ErogameScape (Japanese game database) for game metadata.

**Version:** 0.6.0
**Target Framework:** .NET Framework 4.8
**UI Framework:** WPF with MahApps.Metro

## Quick Reference

```
Solution: Etupirka.sln
Main Project: Etupirka/Etupirka.csproj
Entry Point: App.xaml.cs → MainWindow.xaml.cs
User Database: user.db (SQLite)
Offline Game DB: data.db (SQLite)
```

## Project Structure

```
Etupirka/
├── Etupirka.sln                    # Visual Studio solution
├── Etupirka/
│   ├── App.xaml(.cs)               # Application entry, single-instance check
│   ├── MainWindow.xaml(.cs)        # Main window, process monitoring loop
│   │
│   ├── GameData/                   # Data models
│   │   ├── GameInfo.cs             # Base game metadata
│   │   ├── GameTime.cs             # Game + play-time tracking
│   │   ├── GameExecutionInfo.cs    # Process tracking, status states
│   │   ├── DisplayInfo.cs          # Monitor display settings
│   │   └── TimeData.cs             # Play-time aggregation container
│   │
│   ├── Dialog/                     # Modal dialog windows
│   │   ├── GamePropertyDialog      # Edit game details
│   │   ├── ProcessDialog           # Select game process
│   │   ├── GlobalSettingDialog     # App settings
│   │   ├── DisplaySettingsDialog   # Per-game display config
│   │   ├── PlayTimeStatisticDialog # Statistics view
│   │   ├── GameTimeGraph           # Line chart visualization
│   │   └── AddGameFromESIDDialog   # Add game by ErogameScape ID
│   │
│   ├── Views/                      # Tab pages for statistics
│   │   ├── PlayTimeWeek/Month/30Days/All  # Time period views
│   │   └── GeneralConfig/NetworkConfig/DatabaseConfig  # Settings tabs
│   │
│   ├── Control/
│   │   └── TimeControl.xaml        # Custom time display (HH:MM)
│   │
│   ├── SatoruErogeTimer/           # Legacy data structures
│   │   ├── Eroge.cs                # Simple game-time object
│   │   └── ErogeNode.cs            # Game with execution status
│   │
│   ├── DBManager.cs                # User database operations
│   ├── InformationManager.cs       # Offline ErogameScape database
│   ├── NetworkUtility.cs           # HTTP client, proxy support
│   ├── SystemUtility.cs            # Process/window utilities
│   ├── DisplaySettings.cs          # Windows display API wrapper
│   ├── ProcessInfoCache.cs         # Process path caching
│   ├── Hotkey.cs                   # Global hotkey registration
│   ├── SingleInstance.cs           # Single-instance enforcement
│   ├── StringProcessing.cs         # Title normalization (Levenshtein)
│   └── GridViewSort.cs             # ListView sorting behavior
```

## Key Technologies

| Package | Purpose |
|---------|---------|
| MahApps.Metro 1.6.5 | Metro UI styling |
| System.Data.SQLite 1.0.98.1 | SQLite database |
| HtmlAgilityPack 1.11.34 | HTML parsing for web scraping |
| Newtonsoft.Json 13.0.1 | JSON serialization |
| Hardcodet.NotifyIcon.Wpf 1.0.5 | System tray icon |
| De.TorstenMandelkow.MetroChart | Statistics charts |

## Database Schema

### User Database (user.db)

```sql
-- Game entries
CREATE TABLE games (
    uid TEXT PRIMARY KEY,    -- 16-char random ID
    title TEXT,
    brand TEXT,
    saleday TEXT,            -- yyyy-MM-dd
    esid INTEGER             -- ErogameScape ID (0 if unknown)
);

-- Daily play-time records
CREATE TABLE playtime (
    datetime TEXT,           -- yyyy-MM-dd
    game TEXT,               -- Game UID
    playtime INTEGER,        -- Seconds played
    PRIMARY KEY (datetime, game)
);

-- Lifetime game statistics
CREATE TABLE gametimeinfo (
    uid TEXT PRIMARY KEY,
    playtime INTEGER,        -- Total seconds
    firstplay TEXT,          -- First play timestamp
    lastplay TEXT            -- Last play timestamp
);

-- Execution paths
CREATE TABLE gameexecinfo (
    uid TEXT PRIMARY KEY,
    proc_neq_exec INTEGER,   -- 1 if process != executable
    procpath TEXT,           -- Monitored process path
    execpath TEXT            -- Launch executable path
);

-- Per-monitor display settings
CREATE TABLE gamedisplayinfo (
    uid TEXT,
    device_id TEXT,
    scaling INTEGER,         -- DPI scaling %
    enabled INTEGER,
    PRIMARY KEY (uid, device_id)
);
```

### Offline ErogameScape Database (data.db)

```sql
CREATE TABLE erogamescape (
    id INTEGER PRIMARY KEY,  -- ErogameScape game ID
    title TEXT,
    saleday TEXT,
    brand TEXT
);

CREATE TABLE tableinfo (
    tablename TEXT PRIMARY KEY,
    version INTEGER
);
```

## Core Concepts

### Game Status States (GameExecutionInfo)

```
NotExist  → Game executable not found
Rest      → Process not running
Unfocused → Process running but window not focused
Focused   → Window focused, time is counting
```

### Process Monitoring Loop

Located in `MainWindow.xaml.cs`:
1. Polls every 5 seconds (configurable via `monitorInterval`)
2. Checks if tracked executables are running
3. Uses `GetForegroundWindow` API for focus detection
4. Only counts time when game window is focused
5. Updates database with daily play-time

### ErogameScape Integration

- Scrapes metadata from `erogamescape.dyndns.org/~ap2/ero/toukei_kaiseki/game.php?game=ID`
- Extracts title, brand, release date from HTML
- Optional Google Cache fallback (`useGoogleCache` setting)
- Offline database caching for faster lookups

## Build Instructions

1. Open `Etupirka.sln` in Visual Studio 2019+
2. Restore NuGet packages
3. Build solution (Debug or Release)
4. Output: `Etupirka/bin/Debug/Etupirka.exe`

**Requirements:**
- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- NuGet package restore enabled

## Configuration

Settings in `app.config` and `Properties/Settings.settings`:

| Setting | Type | Description |
|---------|------|-------------|
| watchProcess | bool | Enable process monitoring |
| monitorInterval | int | Check interval (seconds) |
| useOfflineESDatabase | bool | Use cached ES data |
| useGoogleCache | bool | Google Cache fallback |
| minimizeAtStartup | bool | Start minimized to tray |
| askBeforeExit | bool | Show exit confirmation |
| playVoice | bool | Audio feedback on events |

Proxy settings: `proxyType`, `proxyAddress`, `proxyPort`, `proxyUser`, `proxyPass`

## Global Hotkeys

- **Alt+F9**: Toggle process monitoring
- **Alt+F8**: Toggle helper mode

## Common Development Tasks

### Adding a New Dialog

1. Create XAML + code-behind in `Dialog/` folder
2. Use MahApps.Metro `MetroWindow` as base class
3. Wire up from MainWindow or context menu

### Modifying Database Schema

1. Update SQL in `DBManager.cs` (CreateTables method)
2. Add migration logic if needed for existing databases
3. Update corresponding data model classes in `GameData/`

### Adding New Statistics View

1. Create UserControl in `Views/` folder
2. Add tab in parent container (GlobalSettingDialog or statistics dialog)
3. Query data via DBManager methods

## Code Patterns

### Database Access Pattern (DBManager)

```csharp
using (var conn = new SQLiteConnection(connString))
{
    conn.Open();
    using (var cmd = new SQLiteCommand(sql, conn))
    {
        cmd.Parameters.AddWithValue("@param", value);
        // ExecuteNonQuery, ExecuteReader, or ExecuteScalar
    }
}
```

### WPF Data Binding

- ViewModels implement `INotifyPropertyChanged`
- Use `ObservableCollection<T>` for list bindings
- XAML bindings with `{Binding PropertyName}`

### Process Detection

```csharp
Process[] processes = Process.GetProcesses();
foreach (var proc in processes)
{
    // Match against tracked executable paths
    // Use ProcessInfoCache for optimization
}
```

## File Naming Conventions

- **XAML files**: PascalCase with descriptive names (e.g., `GamePropertyDialog.xaml`)
- **Code-behind**: Same name with `.cs` extension
- **Data models**: Singular nouns (e.g., `GameInfo.cs`, `TimeData.cs`)
- **Utilities**: Descriptive with "Utility" or "Manager" suffix

## Notes for Contributors

- UI text is in Japanese; maintain consistency
- Test with various game executable paths (including Unicode paths)
- Database operations should handle missing/corrupt databases gracefully
- Process monitoring must be efficient (runs every 5 seconds)
- MahApps.Metro styles: use `{DynamicResource}` for theme colors
