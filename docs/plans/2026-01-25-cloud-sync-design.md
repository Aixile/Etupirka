# Cloud Sync Design for Etupirka

## Overview

Add cross-machine synchronization of play time data using Cloudflare R2 (S3-compatible storage). Users provide their own R2 credentials - no centralized server needed.

## Goals

- Sync play time data across multiple machines
- No data loss on conflicts (additive merge)
- User controls their own data (BYOB - bring your own bucket)
- Minimal setup friction for technical users

## Data Model

### Play Events

Instead of syncing the SQLite database directly, we use an event-based model. Each machine records play events that can be merged without conflicts.

```csharp
public class PlayEvent
{
    public string MachineId { get; set; }    // Unique per installation
    public string EventId { get; set; }       // MachineId + timestamp hash
    public int GameEsid { get; set; }         // ErogameScape ID (0 if unlinked)
    public string GameUid { get; set; }       // Local UID for manual linking
    public string Date { get; set; }          // yyyy-MM-dd
    public int Seconds { get; set; }          // Play time added
    public DateTime Timestamp { get; set; }   // When event was recorded (UTC)
}
```

### What Gets Synced

| Data | Synced | Notes |
|------|--------|-------|
| Play events | Yes | Core sync data |
| Game metadata | Yes | Only for games with esid > 0 |
| Manual game links | Yes | For games without esid |
| `gameexecinfo` | No | Paths are machine-specific |
| `gamedisplayinfo` | No | Display settings are machine-specific |

### Machine Identity

On first run, Etupirka generates a random `MachineId` stored in settings. This identifies which machine recorded each event.

## R2 Storage Structure

```
bucket/
├── events/
│   ├── {machine_id_1}.json    # Events from machine 1
│   ├── {machine_id_2}.json    # Events from machine 2
│   └── ...
├── games.json                  # Shared game metadata (esid > 0)
└── links.json                  # Manual game links
```

Each machine writes only its own events file. On sync, it reads all machines' events.

## Sync Flow

### Periodic Sync (runs every N minutes, user configurable)

1. **Upload local events** - Append new local play events to `events/{machine_id}.json`
2. **Download all events** - Fetch all `events/*.json` files
3. **Download metadata** - Fetch `games.json` and `links.json`
4. **Merge locally**:
   - Deduplicate events by `EventId`
   - Group by game (via esid or manual link)
   - Recalculate totals per game per day
   - Update local `playtime` and `gametimeinfo` tables
5. **Upload metadata** - If local has newer game info, update `games.json`

### Recording Play Time

Current code calls `UpdateTimeNow()` every 5 seconds. Add a step: append a `PlayEvent` to a local queue. Events are batched and uploaded on next sync.

### First Sync (new machine)

Merge immediately - download cloud data, upload local data, merge everything automatically.

## Game Matching

### Primary: ErogameScape ID

Games are matched across machines by `esid`. If Machine A and Machine B both have a game with esid=4529, they're the same game.

### Fallback: Manual Linking

For games without esid, users can manually link them via a dialog.

**links.json structure:**
```json
{
  "link_abc123": {
    "machines": {
      "machine_1": "local_uid_xyz",
      "machine_2": "local_uid_456"
    },
    "title": "My Untitled Game"
  }
}
```

## User Interface

### Sync Settings Tab

Add a "Sync" tab in `GlobalSettingDialog`:

- Enable/disable cloud sync checkbox
- Provider dropdown (R2 first, extensible later)
- R2 credentials: endpoint, access key, secret key, bucket name
- Sync interval dropdown (1/5/15/30 minutes)
- Test Connection button with status indicator
- Last sync timestamp display
- Machine ID display (read-only)
- Sync Now button

### Link Game Dialog

Context menu option on games without esid: "Link to cloud game"

Options:
- Link to existing unlinked cloud game (from other machines)
- Create new cloud link (other machines can link later)
- Set ErogameScape ID instead (recommended)

### Status Indicator

Subtle indicator in main window showing sync status:
- Green: synced
- Orange: sync pending/failed
- Gray: sync disabled

## Error Handling

| Scenario | Handling |
|----------|----------|
| Network failure | Queue events locally, retry next interval, show orange status |
| Corrupt cloud data | Skip corrupted files, log warning, re-upload local if own file |
| Conflicting metadata | Last timestamp wins (play time never lost) |
| Game deleted locally | Keep events in cloud, add to local exclusion list |
| Wrong bucket permissions | Clear error message on Test Connection |
| Clock skew | Use UTC everywhere, dedupe by EventId hash |

## Implementation

### New Files

```
Etupirka/
├── Sync/
│   ├── ISyncProvider.cs         # Interface for sync backends
│   ├── R2SyncProvider.cs        # Cloudflare R2 implementation
│   ├── SyncManager.cs           # Orchestrates sync logic
│   ├── PlayEvent.cs             # Event data model
│   ├── GameLink.cs              # Manual link data model
│   └── SyncStatus.cs            # Status enum/events for UI
├── Dialog/
│   └── LinkGameDialog.xaml(.cs) # Manual linking dialog
└── Views/
    └── SyncConfigView.xaml(.cs) # Sync settings tab
```

### Dependencies

- `AWSSDK.S3` NuGet package (S3-compatible, works with R2)

### Changes to Existing Files

| File | Change |
|------|--------|
| `MainWindow.xaml.cs` | Add sync timer, status indicator |
| `DBManager.cs` | Add methods to export/import events |
| `GlobalSettingDialog.xaml` | Add Sync tab |
| `Properties/Settings.settings` | Add sync settings |
| `GameExecutionInfo.cs` | Track pending events |

## Future Extensibility

The `ISyncProvider` interface allows adding other backends later:
- Google Drive
- OneDrive
- Dropbox
- WebDAV
- Local folder (for manual sync via cloud drive apps)

## Security Considerations

- API keys stored in user settings (same as existing proxy password handling)
- All data in user's own bucket - they control access
- No data sent to third parties beyond user's chosen provider
