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
