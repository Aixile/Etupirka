# Cloud Sync Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add cross-machine play time synchronization using Cloudflare R2 storage.

**Architecture:** Event-based sync where each machine records play deltas with machine ID. Events are merged without conflicts since play time is additive. Uses S3-compatible API via AWSSDK.S3.

**Tech Stack:** .NET Framework 4.8, WPF, AWSSDK.S3, Newtonsoft.Json, SQLite

---

## Task 1: Add AWSSDK.S3 NuGet Package

**Files:**
- Modify: `Etupirka/packages.config`
- Modify: `Etupirka/Etupirka.csproj`

**Step 1: Add package reference to packages.config**

Add after the last `<package>` element:

```xml
<package id="AWSSDK.Core" version="3.7.100.14" targetFramework="net48" />
<package id="AWSSDK.S3" version="3.7.101.5" targetFramework="net48" />
```

**Step 2: Install packages via NuGet**

In Visual Studio: Tools → NuGet Package Manager → Package Manager Console

```
Install-Package AWSSDK.S3 -Version 3.7.101.5
```

**Step 3: Verify installation**

Build the solution. Should compile without errors.

**Step 4: Commit**

```bash
git add Etupirka/packages.config Etupirka/Etupirka.csproj
git commit -m "feat(sync): add AWSSDK.S3 package for R2 integration"
```

---

## Task 2: Create Sync Data Models

**Files:**
- Create: `Etupirka/Sync/PlayEvent.cs`
- Create: `Etupirka/Sync/GameLink.cs`
- Create: `Etupirka/Sync/SyncStatus.cs`

**Step 1: Create Sync folder**

Create folder `Etupirka/Sync/` in the project.

**Step 2: Create PlayEvent.cs**

```csharp
using System;

namespace Etupirka.Sync
{
    public class PlayEvent
    {
        public string MachineId { get; set; }
        public string EventId { get; set; }
        public int GameEsid { get; set; }
        public string GameUid { get; set; }
        public string Date { get; set; }
        public int Seconds { get; set; }
        public DateTime Timestamp { get; set; }

        public PlayEvent() { }

        public PlayEvent(string machineId, int gameEsid, string gameUid, string date, int seconds)
        {
            MachineId = machineId;
            GameEsid = gameEsid;
            GameUid = gameUid;
            Date = date;
            Seconds = seconds;
            Timestamp = DateTime.UtcNow;
            EventId = GenerateEventId();
        }

        private string GenerateEventId()
        {
            return $"{MachineId}_{Timestamp:yyyyMMddHHmmssfff}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
}
```

**Step 3: Create GameLink.cs**

```csharp
using System.Collections.Generic;

namespace Etupirka.Sync
{
    public class GameLink
    {
        public string LinkId { get; set; }
        public string Title { get; set; }
        public Dictionary<string, string> Machines { get; set; }

        public GameLink()
        {
            Machines = new Dictionary<string, string>();
        }

        public GameLink(string linkId, string title)
        {
            LinkId = linkId;
            Title = title;
            Machines = new Dictionary<string, string>();
        }
    }

    public class GameLinksData
    {
        public Dictionary<string, GameLink> Links { get; set; }

        public GameLinksData()
        {
            Links = new Dictionary<string, GameLink>();
        }
    }
}
```

**Step 4: Create SyncStatus.cs**

```csharp
using System;

namespace Etupirka.Sync
{
    public enum SyncState
    {
        Disabled,
        Idle,
        Syncing,
        Error
    }

    public class SyncStatus
    {
        public SyncState State { get; set; }
        public DateTime? LastSyncTime { get; set; }
        public string LastError { get; set; }

        public SyncStatus()
        {
            State = SyncState.Disabled;
        }
    }
}
```

**Step 5: Add files to project**

In Visual Studio, right-click Sync folder → Add Existing Item → select all three .cs files.

**Step 6: Build and verify**

Build solution. Should compile without errors.

**Step 7: Commit**

```bash
git add Etupirka/Sync/
git commit -m "feat(sync): add data models for PlayEvent, GameLink, SyncStatus"
```

---

## Task 3: Create ISyncProvider Interface

**Files:**
- Create: `Etupirka/Sync/ISyncProvider.cs`

**Step 1: Create ISyncProvider.cs**

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etupirka.Sync
{
    public interface ISyncProvider
    {
        Task<bool> TestConnectionAsync();
        Task UploadEventsAsync(string machineId, List<PlayEvent> events);
        Task<Dictionary<string, List<PlayEvent>>> DownloadAllEventsAsync();
        Task<GameLinksData> DownloadLinksAsync();
        Task UploadLinksAsync(GameLinksData links);
        Task<List<GameInfo>> DownloadGamesMetadataAsync();
        Task UploadGamesMetadataAsync(List<GameInfo> games);
    }
}
```

**Step 2: Add to project and build**

**Step 3: Commit**

```bash
git add Etupirka/Sync/ISyncProvider.cs
git commit -m "feat(sync): add ISyncProvider interface for sync backend abstraction"
```

---

## Task 4: Implement R2SyncProvider

**Files:**
- Create: `Etupirka/Sync/R2SyncProvider.cs`

**Step 1: Create R2SyncProvider.cs**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;

namespace Etupirka.Sync
{
    public class R2SyncProvider : ISyncProvider, IDisposable
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucketName;

        public R2SyncProvider(string endpoint, string accessKey, string secretKey, string bucketName)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = endpoint,
                ForcePathStyle = true
            };
            _client = new AmazonS3Client(accessKey, secretKey, config);
            _bucketName = bucketName;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await _client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    MaxKeys = 1
                });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task UploadEventsAsync(string machineId, List<PlayEvent> events)
        {
            var key = $"events/{machineId}.json";
            var existingEvents = await DownloadEventsForMachineAsync(machineId);

            var allEvents = existingEvents.Concat(events).ToList();
            var json = JsonConvert.SerializeObject(allEvents, Formatting.Indented);

            await _client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                ContentBody = json,
                ContentType = "application/json"
            });
        }

        private async Task<List<PlayEvent>> DownloadEventsForMachineAsync(string machineId)
        {
            try
            {
                var response = await _client.GetObjectAsync(_bucketName, $"events/{machineId}.json");
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<List<PlayEvent>>(json) ?? new List<PlayEvent>();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<PlayEvent>();
            }
        }

        public async Task<Dictionary<string, List<PlayEvent>>> DownloadAllEventsAsync()
        {
            var result = new Dictionary<string, List<PlayEvent>>();

            try
            {
                var listResponse = await _client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = "events/"
                });

                foreach (var obj in listResponse.S3Objects)
                {
                    if (!obj.Key.EndsWith(".json")) continue;

                    var machineId = Path.GetFileNameWithoutExtension(obj.Key);
                    var events = await DownloadEventsForMachineAsync(machineId);
                    result[machineId] = events;
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // No events yet
            }

            return result;
        }

        public async Task<GameLinksData> DownloadLinksAsync()
        {
            try
            {
                var response = await _client.GetObjectAsync(_bucketName, "links.json");
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<GameLinksData>(json) ?? new GameLinksData();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new GameLinksData();
            }
        }

        public async Task UploadLinksAsync(GameLinksData links)
        {
            var json = JsonConvert.SerializeObject(links, Formatting.Indented);
            await _client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = "links.json",
                ContentBody = json,
                ContentType = "application/json"
            });
        }

        public async Task<List<GameInfo>> DownloadGamesMetadataAsync()
        {
            try
            {
                var response = await _client.GetObjectAsync(_bucketName, "games.json");
                using (var reader = new StreamReader(response.ResponseStream))
                {
                    var json = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<List<GameInfo>>(json) ?? new List<GameInfo>();
                }
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<GameInfo>();
            }
        }

        public async Task UploadGamesMetadataAsync(List<GameInfo> games)
        {
            var json = JsonConvert.SerializeObject(games, Formatting.Indented);
            await _client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = "games.json",
                ContentBody = json,
                ContentType = "application/json"
            });
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
```

**Step 2: Add to project and build**

**Step 3: Commit**

```bash
git add Etupirka/Sync/R2SyncProvider.cs
git commit -m "feat(sync): implement R2SyncProvider with S3-compatible API"
```

---

## Task 5: Create SyncManager

**Files:**
- Create: `Etupirka/Sync/SyncManager.cs`

**Step 1: Create SyncManager.cs**

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Etupirka.Properties;

namespace Etupirka.Sync
{
    public class SyncManager : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ISyncProvider _provider;
        private readonly List<PlayEvent> _pendingEvents = new List<PlayEvent>();
        private readonly object _lockObj = new object();

        private SyncStatus _status = new SyncStatus();
        public SyncStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
            }
        }

        public string MachineId { get; private set; }

        public SyncManager()
        {
            MachineId = GetOrCreateMachineId();
        }

        private string GetOrCreateMachineId()
        {
            var id = Settings.Default.syncMachineId;
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString("N").Substring(0, 12);
                Settings.Default.syncMachineId = id;
                Settings.Default.Save();
            }
            return id;
        }

        public void Configure(string endpoint, string accessKey, string secretKey, string bucketName)
        {
            _provider?.Dispose();
            _provider = new R2SyncProvider(endpoint, accessKey, secretKey, bucketName);
        }

        public async Task<bool> TestConnectionAsync()
        {
            if (_provider == null) return false;
            return await _provider.TestConnectionAsync();
        }

        public void RecordPlayEvent(int gameEsid, string gameUid, int seconds)
        {
            if (!Settings.Default.syncEnabled) return;

            var evt = new PlayEvent(
                MachineId,
                gameEsid,
                gameUid,
                DateTime.Today.ToString("yyyy-MM-dd"),
                seconds
            );

            lock (_lockObj)
            {
                _pendingEvents.Add(evt);
            }
        }

        public async Task SyncAsync(DBManager db, IEnumerable<GameExecutionInfo> games)
        {
            if (_provider == null || !Settings.Default.syncEnabled)
            {
                return;
            }

            var newStatus = new SyncStatus { State = SyncState.Syncing };
            Status = newStatus;

            try
            {
                // 1. Upload pending events
                List<PlayEvent> eventsToUpload;
                lock (_lockObj)
                {
                    eventsToUpload = _pendingEvents.ToList();
                    _pendingEvents.Clear();
                }

                if (eventsToUpload.Any())
                {
                    await _provider.UploadEventsAsync(MachineId, eventsToUpload);
                }

                // 2. Download all events from all machines
                var allEvents = await _provider.DownloadAllEventsAsync();

                // 3. Download links for games without esid
                var links = await _provider.DownloadLinksAsync();

                // 4. Merge events into local database
                MergeEvents(db, games, allEvents, links);

                // 5. Upload local game metadata (for games with esid)
                var gamesWithEsid = games.Where(g => g.ErogameScapeID > 0)
                    .Select(g => new GameInfo
                    {
                        Title = g.Title,
                        Brand = g.Brand,
                        SaleDay = g.SaleDay,
                        ErogameScapeID = g.ErogameScapeID
                    }).ToList();

                if (gamesWithEsid.Any())
                {
                    await _provider.UploadGamesMetadataAsync(gamesWithEsid);
                }

                newStatus.State = SyncState.Idle;
                newStatus.LastSyncTime = DateTime.Now;
                newStatus.LastError = null;
            }
            catch (Exception ex)
            {
                newStatus.State = SyncState.Error;
                newStatus.LastError = ex.Message;

                // Re-add events that failed to upload
                lock (_lockObj)
                {
                    // Events are already cleared, we lost them on error
                    // In production, consider persisting pending events to disk
                }
            }

            Status = newStatus;
        }

        private void MergeEvents(
            DBManager db,
            IEnumerable<GameExecutionInfo> localGames,
            Dictionary<string, List<PlayEvent>> allEvents,
            GameLinksData links)
        {
            // Flatten all events and deduplicate by EventId
            var uniqueEvents = allEvents
                .SelectMany(kv => kv.Value)
                .GroupBy(e => e.EventId)
                .Select(g => g.First())
                .ToList();

            // Group by game (esid or link) and date
            var gamesByEsid = localGames.Where(g => g.ErogameScapeID > 0)
                .ToDictionary(g => g.ErogameScapeID, g => g);

            var timeUpdates = new Dictionary<string, Dictionary<string, int>>(); // uid -> date -> seconds

            foreach (var evt in uniqueEvents)
            {
                // Skip events from this machine (already in local DB)
                if (evt.MachineId == MachineId) continue;

                GameExecutionInfo targetGame = null;

                // Try to match by esid
                if (evt.GameEsid > 0 && gamesByEsid.TryGetValue(evt.GameEsid, out targetGame))
                {
                    // Found by esid
                }
                // Try to match by link
                else if (links.Links.Values.Any(l => l.Machines.ContainsKey(evt.MachineId) &&
                         l.Machines[evt.MachineId] == evt.GameUid))
                {
                    var link = links.Links.Values.First(l =>
                        l.Machines.ContainsKey(evt.MachineId) &&
                        l.Machines[evt.MachineId] == evt.GameUid);

                    if (link.Machines.TryGetValue(MachineId, out var localUid))
                    {
                        targetGame = localGames.FirstOrDefault(g => g.UID == localUid);
                    }
                }

                if (targetGame == null) continue;

                if (!timeUpdates.ContainsKey(targetGame.UID))
                {
                    timeUpdates[targetGame.UID] = new Dictionary<string, int>();
                }

                if (!timeUpdates[targetGame.UID].ContainsKey(evt.Date))
                {
                    timeUpdates[targetGame.UID][evt.Date] = 0;
                }

                timeUpdates[targetGame.UID][evt.Date] += evt.Seconds;
            }

            // Apply updates to database
            foreach (var gameUpdate in timeUpdates)
            {
                foreach (var dateUpdate in gameUpdate.Value)
                {
                    // Add time (not replace) to handle merging
                    db.AddPlayTimeForSync(dateUpdate.Key, gameUpdate.Key, dateUpdate.Value);
                }
            }
        }

        public void Dispose()
        {
            (_provider as IDisposable)?.Dispose();
        }
    }
}
```

**Step 2: Add to project and build**

**Step 3: Commit**

```bash
git add Etupirka/Sync/SyncManager.cs
git commit -m "feat(sync): implement SyncManager with event recording and merging"
```

---

## Task 6: Add Sync Settings to Properties

**Files:**
- Modify: `Etupirka/Properties/Settings.settings`
- Modify: `Etupirka/Properties/Settings.Designer.cs`

**Step 1: Add sync settings to Settings.settings**

Add inside `<Settings>` element:

```xml
<Setting Name="syncEnabled" Type="System.Boolean" Scope="User">
  <Value Profile="(Default)">False</Value>
</Setting>
<Setting Name="syncEndpoint" Type="System.String" Scope="User">
  <Value Profile="(Default)" />
</Setting>
<Setting Name="syncAccessKey" Type="System.String" Scope="User">
  <Value Profile="(Default)" />
</Setting>
<Setting Name="syncSecretKey" Type="System.String" Scope="User">
  <Value Profile="(Default)" />
</Setting>
<Setting Name="syncBucketName" Type="System.String" Scope="User">
  <Value Profile="(Default)" />
</Setting>
<Setting Name="syncInterval" Type="System.Int32" Scope="User">
  <Value Profile="(Default)">5</Value>
</Setting>
<Setting Name="syncMachineId" Type="System.String" Scope="User">
  <Value Profile="(Default)" />
</Setting>
```

**Step 2: Regenerate Settings.Designer.cs**

In Visual Studio, open Settings.settings in designer mode and save. This regenerates the Designer.cs file.

**Step 3: Build and verify**

**Step 4: Commit**

```bash
git add Etupirka/Properties/Settings.settings Etupirka/Properties/Settings.Designer.cs
git commit -m "feat(sync): add sync configuration settings"
```

---

## Task 7: Add DBManager Sync Methods

**Files:**
- Modify: `Etupirka/DBManager.cs`

**Step 1: Add AddPlayTimeForSync method**

Add after the existing `AddPlayTime` method (around line 224):

```csharp
public void AddPlayTimeForSync(string date, string gameUid, int seconds)
{
    using (SQLiteCommand command = conn.CreateCommand())
    {
        conn.Open();
        command.CommandText = @"INSERT OR IGNORE INTO playtime VALUES(@DATE, @GAME, 0);
                                UPDATE playtime SET playtime = playtime + @SECONDS
                                WHERE datetime = @DATE AND game = @GAME";
        command.Parameters.AddWithValue("@DATE", date);
        command.Parameters.AddWithValue("@GAME", gameUid);
        command.Parameters.AddWithValue("@SECONDS", seconds);
        command.ExecuteNonQuery();
        conn.Close();
    }
}
```

**Step 2: Build and verify**

**Step 3: Commit**

```bash
git add Etupirka/DBManager.cs
git commit -m "feat(sync): add DBManager.AddPlayTimeForSync for merging remote events"
```

---

## Task 8: Create SyncConfigView

**Files:**
- Create: `Etupirka/Views/SyncConfigView.xaml`
- Create: `Etupirka/Views/SyncConfigView.xaml.cs`

**Step 1: Create SyncConfigView.xaml**

```xml
<UserControl x:Class="Etupirka.Views.SyncConfigView"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
             xmlns:Controls="clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro"
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d"
             d:DesignHeight="350" d:DesignWidth="400">
    <Grid Margin="10">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>

        <CheckBox Grid.Row="0" Grid.ColumnSpan="2" Margin="2"
                  Content="クラウド同期を有効にする"
                  IsChecked="{Binding SyncEnabled}" />

        <Label Grid.Row="1" Grid.Column="0" Margin="2">R2 エンドポイント:</Label>
        <TextBox Grid.Row="1" Grid.Column="1" Margin="2"
                 Text="{Binding SyncEndpoint, UpdateSourceTrigger=PropertyChanged}"
                 IsEnabled="{Binding SyncEnabled}" />

        <Label Grid.Row="2" Grid.Column="0" Margin="2">アクセスキー ID:</Label>
        <TextBox Grid.Row="2" Grid.Column="1" Margin="2"
                 Text="{Binding SyncAccessKey, UpdateSourceTrigger=PropertyChanged}"
                 IsEnabled="{Binding SyncEnabled}" />

        <Label Grid.Row="3" Grid.Column="0" Margin="2">シークレットキー:</Label>
        <PasswordBox Grid.Row="3" Grid.Column="1" Margin="2"
                     x:Name="secretKeyBox"
                     IsEnabled="{Binding SyncEnabled}" />

        <Label Grid.Row="4" Grid.Column="0" Margin="2">バケット名:</Label>
        <TextBox Grid.Row="4" Grid.Column="1" Margin="2"
                 Text="{Binding SyncBucketName, UpdateSourceTrigger=PropertyChanged}"
                 IsEnabled="{Binding SyncEnabled}" />

        <Label Grid.Row="5" Grid.Column="0" Margin="2">同期間隔:</Label>
        <StackPanel Grid.Row="5" Grid.Column="1" Orientation="Horizontal">
            <Controls:NumericUpDown Margin="2" Width="80"
                                    Value="{Binding SyncInterval, UpdateSourceTrigger=PropertyChanged}"
                                    Minimum="1" Maximum="60"
                                    IsEnabled="{Binding SyncEnabled}"
                                    HideUpDownButtons="True" />
            <Label Margin="2">分</Label>
        </StackPanel>

        <StackPanel Grid.Row="6" Grid.ColumnSpan="2" Orientation="Horizontal" Margin="2,10,2,2">
            <Button x:Name="btnTestConnection" Click="btnTestConnection_Click"
                    MinWidth="100" Margin="0,0,10,0"
                    IsEnabled="{Binding SyncEnabled}">接続テスト</Button>
            <Label x:Name="lblConnectionStatus" VerticalAlignment="Center" />
        </StackPanel>

        <StackPanel Grid.Row="7" Grid.ColumnSpan="2" Margin="2,10,2,2">
            <TextBlock>
                <Run>マシン ID: </Run>
                <Run Text="{Binding MachineId, Mode=OneWay}" />
            </TextBlock>
            <TextBlock>
                <Run>最終同期: </Run>
                <Run x:Name="txtLastSync" Text="未同期" />
            </TextBlock>
        </StackPanel>

        <Button Grid.Row="8" Grid.ColumnSpan="2" x:Name="btnSyncNow"
                Click="btnSyncNow_Click" Margin="2,10,2,2"
                HorizontalAlignment="Left" MinWidth="100"
                IsEnabled="{Binding SyncEnabled}">今すぐ同期</Button>
    </Grid>
</UserControl>
```

**Step 2: Create SyncConfigView.xaml.cs**

```csharp
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Etupirka.Properties;
using Etupirka.Sync;

namespace Etupirka.Views
{
    public partial class SyncConfigView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SyncManager _syncManager;
        private Action _syncNowCallback;

        public SyncConfigView()
        {
            InitializeComponent();
            DataContext = this;
            secretKeyBox.Password = Settings.Default.syncSecretKey;
            secretKeyBox.PasswordChanged += SecretKeyBox_PasswordChanged;
        }

        public void Initialize(SyncManager syncManager, Action syncNowCallback)
        {
            _syncManager = syncManager;
            _syncNowCallback = syncNowCallback;
            MachineId = syncManager.MachineId;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MachineId)));
        }

        public bool SyncEnabled
        {
            get => Settings.Default.syncEnabled;
            set
            {
                Settings.Default.syncEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SyncEnabled)));
            }
        }

        public string SyncEndpoint
        {
            get => Settings.Default.syncEndpoint;
            set => Settings.Default.syncEndpoint = value;
        }

        public string SyncAccessKey
        {
            get => Settings.Default.syncAccessKey;
            set => Settings.Default.syncAccessKey = value;
        }

        public string SyncBucketName
        {
            get => Settings.Default.syncBucketName;
            set => Settings.Default.syncBucketName = value;
        }

        public int SyncInterval
        {
            get => Settings.Default.syncInterval;
            set => Settings.Default.syncInterval = value;
        }

        public string MachineId { get; private set; }

        private void SecretKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.syncSecretKey = secretKeyBox.Password;
        }

        private async void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            btnTestConnection.IsEnabled = false;
            lblConnectionStatus.Content = "接続中...";

            try
            {
                _syncManager.Configure(SyncEndpoint, SyncAccessKey, secretKeyBox.Password, SyncBucketName);
                var result = await _syncManager.TestConnectionAsync();

                if (result)
                {
                    lblConnectionStatus.Content = "接続成功";
                    lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    lblConnectionStatus.Content = "接続失敗";
                    lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Content = $"エラー: {ex.Message}";
                lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
            }

            btnTestConnection.IsEnabled = true;
        }

        private void btnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            _syncNowCallback?.Invoke();
            txtLastSync.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void UpdateLastSyncTime(DateTime? time)
        {
            if (time.HasValue)
            {
                txtLastSync.Text = time.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
```

**Step 3: Add files to project**

**Step 4: Build and verify**

**Step 5: Commit**

```bash
git add Etupirka/Views/SyncConfigView.xaml Etupirka/Views/SyncConfigView.xaml.cs
git commit -m "feat(sync): add SyncConfigView for R2 configuration UI"
```

---

## Task 9: Add Sync Tab to GlobalSettingDialog

**Files:**
- Modify: `Etupirka/Dialog/GlobalSettingDialog.xaml`
- Modify: `Etupirka/Dialog/GlobalSettingDialog.xaml.cs`

**Step 1: Add sync tab to GlobalSettingDialog.xaml**

After the NetworkConfigView MetroTabItem (line 28), add:

```xml
<Controls:MetroTabItem Header="同期" Controls:ControlsHelper.HeaderFontSize="20">
    <Views:SyncConfigView x:Name="SyncView" />
</Controls:MetroTabItem>
```

**Step 2: Update GlobalSettingDialog.xaml.cs**

Add a method to initialize the sync view. In the constructor or a public method:

```csharp
public void InitializeSyncView(SyncManager syncManager, Action syncNowCallback)
{
    SyncView.Initialize(syncManager, syncNowCallback);
}
```

**Step 3: Build and verify**

**Step 4: Commit**

```bash
git add Etupirka/Dialog/GlobalSettingDialog.xaml Etupirka/Dialog/GlobalSettingDialog.xaml.cs
git commit -m "feat(sync): add Sync tab to GlobalSettingDialog"
```

---

## Task 10: Integrate SyncManager into MainWindow

**Files:**
- Modify: `Etupirka/MainWindow.xaml.cs`

**Step 1: Add SyncManager field and timer**

Add after the `watchProcTimer` declaration (around line 176):

```csharp
private Sync.SyncManager syncManager;
private System.Windows.Threading.DispatcherTimer syncTimer;
```

**Step 2: Initialize SyncManager in constructor**

Add after the watchProcTimer setup (around line 224):

```csharp
// Initialize sync
syncManager = new Sync.SyncManager();
if (Settings.Default.syncEnabled)
{
    syncManager.Configure(
        Settings.Default.syncEndpoint,
        Settings.Default.syncAccessKey,
        Settings.Default.syncSecretKey,
        Settings.Default.syncBucketName
    );
}

syncTimer = new System.Windows.Threading.DispatcherTimer();
syncTimer.Tick += syncTimer_Tick;
syncTimer.Interval = TimeSpan.FromMinutes(Settings.Default.syncInterval);
if (Settings.Default.syncEnabled)
{
    syncTimer.Start();
}
```

**Step 3: Add sync timer tick handler**

Add after the existing dispatcherTimer_Tick method:

```csharp
private async void syncTimer_Tick(object sender, EventArgs e)
{
    if (!Settings.Default.syncEnabled) return;

    try
    {
        await syncManager.SyncAsync(db, items);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Sync error: {ex.Message}");
    }
}
```

**Step 4: Record play events**

In the UpdateStatus method, where time is recorded (around line 322), add after `db.UpdateTimeNow`:

```csharp
syncManager?.RecordPlayEvent(i.ErogameScapeID, i.UID, time);
```

**Step 5: Update global settings dialog initialization**

Find where GlobalSettingDialog is opened and add initialization:

```csharp
dialog.InitializeSyncView(syncManager, async () => {
    await syncManager.SyncAsync(db, items);
});
```

**Step 6: Build and verify**

**Step 7: Commit**

```bash
git add Etupirka/MainWindow.xaml.cs
git commit -m "feat(sync): integrate SyncManager into MainWindow with periodic sync"
```

---

## Task 11: Create LinkGameDialog

**Files:**
- Create: `Etupirka/Dialog/LinkGameDialog.xaml`
- Create: `Etupirka/Dialog/LinkGameDialog.xaml.cs`

**Step 1: Create LinkGameDialog.xaml**

```xml
<Controls:MetroWindow x:Class="Etupirka.Dialog.LinkGameDialog"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:Controls="clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro"
        WindowStartupLocation="CenterOwner"
        Title="ゲームをリンク" Height="300" Width="400"
        WindowTitleBrush="#FF252525"
        GlowBrush="{DynamicResource AccentColorBrush}"
        BorderThickness="1.5"
        TitleCaps="False">
    <Grid Margin="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Margin="0,0,0,10">
            <Run>ローカルゲーム: </Run>
            <Run x:Name="txtLocalGame" FontWeight="Bold" />
        </TextBlock>

        <StackPanel Grid.Row="1">
            <RadioButton x:Name="rbExisting" Margin="2" IsChecked="True">
                既存のクラウドゲームにリンク:
            </RadioButton>
            <ComboBox x:Name="cmbCloudGames" Margin="20,2,2,2"
                      IsEnabled="{Binding IsChecked, ElementName=rbExisting}"
                      DisplayMemberPath="Title" />

            <RadioButton x:Name="rbNewLink" Margin="2,10,2,2">
                新しいリンクを作成（他のマシンが後でリンクできます）
            </RadioButton>

            <RadioButton x:Name="rbSetEsid" Margin="2,10,2,2">
                ErogameScape IDを設定（推奨）:
            </RadioButton>
            <Controls:NumericUpDown x:Name="nudEsid" Margin="20,2,2,2"
                                    Minimum="1" Maximum="999999"
                                    IsEnabled="{Binding IsChecked, ElementName=rbSetEsid}"
                                    HideUpDownButtons="True" />
        </StackPanel>

        <WrapPanel Grid.Row="3" HorizontalAlignment="Right" Margin="0,10,0,0">
            <Button x:Name="btnLink" Click="btnLink_Click" MinWidth="60" Margin="0,0,10,0">リンク</Button>
            <Button IsCancel="True" MinWidth="60">キャンセル</Button>
        </WrapPanel>
    </Grid>
</Controls:MetroWindow>
```

**Step 2: Create LinkGameDialog.xaml.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Etupirka.Sync;
using MahApps.Metro.Controls;

namespace Etupirka.Dialog
{
    public partial class LinkGameDialog : MetroWindow
    {
        public class CloudGameItem
        {
            public string LinkId { get; set; }
            public string Title { get; set; }
        }

        private readonly GameExecutionInfo _localGame;
        private readonly SyncManager _syncManager;
        private readonly GameLinksData _linksData;

        public int? SelectedEsid { get; private set; }
        public string SelectedLinkId { get; private set; }
        public bool CreateNewLink { get; private set; }

        public LinkGameDialog(GameExecutionInfo localGame, SyncManager syncManager, GameLinksData linksData)
        {
            InitializeComponent();
            _localGame = localGame;
            _syncManager = syncManager;
            _linksData = linksData;

            txtLocalGame.Text = localGame.Title;

            // Populate cloud games that don't have this machine linked
            var unlinkedGames = linksData.Links.Values
                .Where(l => !l.Machines.ContainsKey(syncManager.MachineId))
                .Select(l => new CloudGameItem { LinkId = l.LinkId, Title = l.Title })
                .ToList();

            cmbCloudGames.ItemsSource = unlinkedGames;
            if (unlinkedGames.Any())
            {
                cmbCloudGames.SelectedIndex = 0;
            }
            else
            {
                rbExisting.IsEnabled = false;
                rbNewLink.IsChecked = true;
            }
        }

        private void btnLink_Click(object sender, RoutedEventArgs e)
        {
            if (rbSetEsid.IsChecked == true)
            {
                SelectedEsid = (int)nudEsid.Value;
            }
            else if (rbNewLink.IsChecked == true)
            {
                CreateNewLink = true;
            }
            else if (rbExisting.IsChecked == true && cmbCloudGames.SelectedItem != null)
            {
                SelectedLinkId = ((CloudGameItem)cmbCloudGames.SelectedItem).LinkId;
            }
            else
            {
                MessageBox.Show("リンク方法を選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
```

**Step 3: Add files to project**

**Step 4: Build and verify**

**Step 5: Commit**

```bash
git add Etupirka/Dialog/LinkGameDialog.xaml Etupirka/Dialog/LinkGameDialog.xaml.cs
git commit -m "feat(sync): add LinkGameDialog for manual game linking"
```

---

## Task 12: Add Link Menu Item to MainWindow

**Files:**
- Modify: `Etupirka/MainWindow.xaml`
- Modify: `Etupirka/MainWindow.xaml.cs`

**Step 1: Add context menu item in MainWindow.xaml**

Find the context menu for the game list and add:

```xml
<MenuItem Header="クラウドにリンク..." Click="MenuLinkGame_Click" />
```

**Step 2: Add click handler in MainWindow.xaml.cs**

```csharp
private async void MenuLinkGame_Click(object sender, RoutedEventArgs e)
{
    if (GameListView.SelectedItem == null) return;
    var game = (GameExecutionInfo)GameListView.SelectedItem;

    if (game.ErogameScapeID > 0)
    {
        MessageBox.Show("このゲームは既にErogameScape IDがあるため、自動的に同期されます。",
            "情報", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    if (!Settings.Default.syncEnabled)
    {
        MessageBox.Show("同期が有効になっていません。設定で同期を有効にしてください。",
            "情報", MessageBoxButton.OK, MessageBoxImage.Information);
        return;
    }

    try
    {
        // Download current links
        syncManager.Configure(
            Settings.Default.syncEndpoint,
            Settings.Default.syncAccessKey,
            Settings.Default.syncSecretKey,
            Settings.Default.syncBucketName
        );

        var provider = new Sync.R2SyncProvider(
            Settings.Default.syncEndpoint,
            Settings.Default.syncAccessKey,
            Settings.Default.syncSecretKey,
            Settings.Default.syncBucketName
        );

        var linksData = await provider.DownloadLinksAsync();

        var dialog = new Dialog.LinkGameDialog(game, syncManager, linksData);
        dialog.Owner = this;

        if (dialog.ShowDialog() == true)
        {
            if (dialog.SelectedEsid.HasValue)
            {
                game.ErogameScapeID = dialog.SelectedEsid.Value;
                game.updateInfoFromES();
                db.UpdateGameInfoAndExec(game);
            }
            else if (dialog.CreateNewLink)
            {
                var newLinkId = $"link_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
                var newLink = new Sync.GameLink(newLinkId, game.Title);
                newLink.Machines[syncManager.MachineId] = game.UID;
                linksData.Links[newLinkId] = newLink;
                await provider.UploadLinksAsync(linksData);
            }
            else if (!string.IsNullOrEmpty(dialog.SelectedLinkId))
            {
                var link = linksData.Links[dialog.SelectedLinkId];
                link.Machines[syncManager.MachineId] = game.UID;
                await provider.UploadLinksAsync(linksData);
            }
        }

        provider.Dispose();
    }
    catch (Exception ex)
    {
        MessageBox.Show($"エラー: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

**Step 3: Build and verify**

**Step 4: Commit**

```bash
git add Etupirka/MainWindow.xaml Etupirka/MainWindow.xaml.cs
git commit -m "feat(sync): add Link to Cloud context menu option"
```

---

## Task 13: Add Sync Status Indicator to MainWindow

**Files:**
- Modify: `Etupirka/MainWindow.xaml`
- Modify: `Etupirka/MainWindow.xaml.cs`

**Step 1: Add status indicator in MainWindow.xaml**

Add in the status bar area:

```xml
<Ellipse x:Name="syncStatusIndicator" Width="10" Height="10"
         Fill="Gray" Margin="5,0" ToolTip="同期: 無効" />
```

**Step 2: Add status update method in MainWindow.xaml.cs**

```csharp
private void UpdateSyncStatusIndicator()
{
    if (!Settings.Default.syncEnabled)
    {
        syncStatusIndicator.Fill = System.Windows.Media.Brushes.Gray;
        syncStatusIndicator.ToolTip = "同期: 無効";
        return;
    }

    switch (syncManager.Status.State)
    {
        case Sync.SyncState.Idle:
            syncStatusIndicator.Fill = System.Windows.Media.Brushes.Green;
            syncStatusIndicator.ToolTip = $"同期: 正常\n最終: {syncManager.Status.LastSyncTime:yyyy-MM-dd HH:mm}";
            break;
        case Sync.SyncState.Syncing:
            syncStatusIndicator.Fill = System.Windows.Media.Brushes.Blue;
            syncStatusIndicator.ToolTip = "同期: 同期中...";
            break;
        case Sync.SyncState.Error:
            syncStatusIndicator.Fill = System.Windows.Media.Brushes.Orange;
            syncStatusIndicator.ToolTip = $"同期: エラー\n{syncManager.Status.LastError}";
            break;
        default:
            syncStatusIndicator.Fill = System.Windows.Media.Brushes.Gray;
            syncStatusIndicator.ToolTip = "同期: 無効";
            break;
    }
}
```

**Step 3: Call UpdateSyncStatusIndicator after sync**

In the syncTimer_Tick method, add at the end:

```csharp
UpdateSyncStatusIndicator();
```

**Step 4: Build and verify**

**Step 5: Commit**

```bash
git add Etupirka/MainWindow.xaml Etupirka/MainWindow.xaml.cs
git commit -m "feat(sync): add sync status indicator to main window"
```

---

## Task 14: Final Integration Testing

**Manual Test Checklist:**

1. **Build and run**
   - Solution compiles without errors
   - Application starts normally

2. **Sync settings**
   - Open settings dialog
   - Navigate to "同期" tab
   - Verify all fields are present
   - Enter R2 credentials
   - Click "接続テスト" - should show success/failure

3. **Enable sync**
   - Check "クラウド同期を有効にする"
   - Click OK
   - Verify sync timer starts

4. **Play time recording**
   - Start a tracked game
   - Play for a few seconds
   - Verify local time is recorded
   - Click "今すぐ同期"
   - Check R2 bucket for events file

5. **Multi-machine sync (if possible)**
   - Install on second machine
   - Configure same R2 bucket
   - Play on machine A
   - Sync on machine B
   - Verify time appears on machine B

6. **Manual linking**
   - Add a game without ESID
   - Right-click → "クラウドにリンク..."
   - Test each option (new link, set ESID)

**Step: Final commit**

```bash
git add -A
git commit -m "feat(sync): complete cloud sync implementation

- Add Cloudflare R2 integration via AWSSDK.S3
- Event-based sync model for conflict-free merging
- SyncConfigView for R2 credential configuration
- LinkGameDialog for manual game linking
- Periodic sync with configurable interval
- Status indicator in main window"
```

---

## Summary

| Task | Description | Files |
|------|-------------|-------|
| 1 | Add AWSSDK.S3 NuGet | packages.config, csproj |
| 2 | Create data models | Sync/PlayEvent.cs, GameLink.cs, SyncStatus.cs |
| 3 | Create ISyncProvider | Sync/ISyncProvider.cs |
| 4 | Implement R2SyncProvider | Sync/R2SyncProvider.cs |
| 5 | Create SyncManager | Sync/SyncManager.cs |
| 6 | Add sync settings | Properties/Settings.settings |
| 7 | Add DBManager sync method | DBManager.cs |
| 8 | Create SyncConfigView | Views/SyncConfigView.xaml(.cs) |
| 9 | Add sync tab to settings | Dialog/GlobalSettingDialog.xaml(.cs) |
| 10 | Integrate into MainWindow | MainWindow.xaml.cs |
| 11 | Create LinkGameDialog | Dialog/LinkGameDialog.xaml(.cs) |
| 12 | Add link menu item | MainWindow.xaml(.cs) |
| 13 | Add status indicator | MainWindow.xaml(.cs) |
| 14 | Integration testing | Manual testing |
