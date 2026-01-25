using System;

namespace Etupirka.Sync
{
    /// <summary>
    /// Simplified game info for cloud sync (JSON serialization)
    /// </summary>
    public class GameInfo
    {
        public int ErogameScapeID { get; set; }
        public string Title { get; set; }
        public string Brand { get; set; }
        public string SaleDay { get; set; }

        public GameInfo()
        {
            Title = "";
            Brand = "";
            SaleDay = "";
        }
    }
}
