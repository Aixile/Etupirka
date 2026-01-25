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
