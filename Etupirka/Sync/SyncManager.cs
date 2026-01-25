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
        private DBManager _db;

        public SyncManager()
        {
            MachineId = GetOrCreateMachineId();
        }

        internal void SetDatabase(DBManager db)
        {
            _db = db;
            LoadLastSyncTime();
        }

        private void LoadLastSyncTime()
        {
            if (_db == null) return;
            try
            {
                var lastSyncStr = _db.GetSyncInfo("LastSyncTime");
                if (!string.IsNullOrEmpty(lastSyncStr) && DateTime.TryParse(lastSyncStr, out var lastSync))
                {
                    _status.LastSyncTime = lastSync;
                }
            }
            catch { }
        }

        private void SaveLastSyncTime(DateTime time)
        {
            if (_db == null) return;
            try
            {
                _db.SetSyncInfo("LastSyncTime", time.ToString("o"));
            }
            catch { }
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
            (_provider as IDisposable)?.Dispose();
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

        internal async Task SyncAsync(DBManager db, IEnumerable<GameExecutionInfo> games, Action<GameExecutionInfo> onGameCreated = null)
        {
            if (_provider == null || !Settings.Default.syncEnabled)
            {
                return;
            }

            var newStatus = new SyncStatus { State = SyncState.Syncing };
            Status = newStatus;

            try
            {
                // 1. Upload pending events (aggregated by game+date)
                List<PlayEvent> eventsToUpload;
                lock (_lockObj)
                {
                    // Aggregate events by game+date to reduce data size
                    eventsToUpload = _pendingEvents
                        .GroupBy(e => new { e.GameEsid, e.GameUid, e.Date })
                        .Select(g => new PlayEvent(
                            MachineId,
                            g.Key.GameEsid,
                            g.Key.GameUid,
                            g.Key.Date,
                            g.Sum(e => e.Seconds)))
                        .ToList();
                    _pendingEvents.Clear();
                }

                if (eventsToUpload.Any())
                {
                    await _provider.UploadEventsAsync(MachineId, eventsToUpload);
                }

                // 2. Download cloud games and auto-create missing ones
                var gamesList = games.ToList();
                var cloudGames = await _provider.DownloadAllGamesMetadataAsync();
                foreach (var cloudGame in cloudGames)
                {
                    if (cloudGame.ErogameScapeID <= 0) continue;
                    if (db.GameExistsByEsid(cloudGame.ErogameScapeID)) continue;

                    // Create new local game entry
                    var uid = db.CreateGameFromCloud(cloudGame);
                    var newGame = new GameExecutionInfo
                    {
                        UID = uid,
                        Title = cloudGame.Title,
                        Brand = cloudGame.Brand,
                        ErogameScapeID = cloudGame.ErogameScapeID
                    };
                    if (DateTime.TryParse(cloudGame.SaleDay, out var saleDay))
                    {
                        newGame.SaleDay = saleDay;
                    }
                    gamesList.Add(newGame);
                    onGameCreated?.Invoke(newGame);
                }

                // 3. Download all events from all machines
                var allEvents = await _provider.DownloadAllEventsAsync();

                // 4. Download links for games without esid
                var links = await _provider.DownloadLinksAsync();

                // 5. Merge events into local database
                MergeEvents(db, gamesList, allEvents, links);

                // 5. Upload local game metadata (for games with esid)
                var gamesWithEsid = games.Where(g => g.ErogameScapeID > 0)
                    .Select(g => new GameInfo
                    {
                        Title = g.Title,
                        Brand = g.Brand,
                        SaleDay = g.SaleDay.Ticks != 0 ? g.SaleDay.ToString("yyyy-MM-dd") : "",
                        ErogameScapeID = g.ErogameScapeID
                    }).ToList();

                if (gamesWithEsid.Any())
                {
                    await _provider.UploadGamesMetadataAsync(MachineId, gamesWithEsid);
                }

                newStatus.State = SyncState.Idle;
                newStatus.LastSyncTime = DateTime.Now;
                newStatus.LastError = null;
                SaveLastSyncTime(newStatus.LastSyncTime.Value);
            }
            catch (Exception ex)
            {
                newStatus.State = SyncState.Error;
                newStatus.LastError = ex.Message;
            }

            Status = newStatus;
        }

        internal async Task<int> ImportFromCloudAsync(DBManager db, IEnumerable<GameExecutionInfo> localGames, Action<GameExecutionInfo> onGameCreated)
        {
            if (_provider == null)
            {
                throw new InvalidOperationException("Sync provider not configured");
            }

            int importedCount = 0;
            var gamesList = localGames.ToList();

            // 1. Download game metadata from cloud
            var cloudGames = await _provider.DownloadAllGamesMetadataAsync();

            // 2. For each game with ESID that doesn't exist locally, create it
            foreach (var cloudGame in cloudGames)
            {
                if (cloudGame.ErogameScapeID <= 0) continue;
                if (db.GameExistsByEsid(cloudGame.ErogameScapeID)) continue;

                // Create new local game entry
                var uid = db.CreateGameFromCloud(cloudGame);

                // Create GameExecutionInfo for UI update
                var newGame = new GameExecutionInfo
                {
                    UID = uid,
                    Title = cloudGame.Title,
                    Brand = cloudGame.Brand,
                    ErogameScapeID = cloudGame.ErogameScapeID
                };

                if (DateTime.TryParse(cloudGame.SaleDay, out var saleDay))
                {
                    newGame.SaleDay = saleDay;
                }

                gamesList.Add(newGame);
                onGameCreated?.Invoke(newGame);
                importedCount++;
            }

            // 3. Download all events and merge play time
            var allEvents = await _provider.DownloadAllEventsAsync();
            var links = await _provider.DownloadLinksAsync();

            // Merge events (including for newly created games)
            MergeEvents(db, gamesList, allEvents, links);

            return importedCount;
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
