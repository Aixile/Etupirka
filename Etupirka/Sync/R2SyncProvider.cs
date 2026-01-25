using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
