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
