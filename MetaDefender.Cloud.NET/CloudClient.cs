using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Polly;

namespace MetaDefender.Cloud.NET
{
    public class CloudClient
    {
        private readonly HttpClient _httpClient;

        public CloudClient(string apiKey)
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("https://api.metadefender.com/v4/")};
            _httpClient.DefaultRequestHeaders.Add("apikey", apiKey);
        }

        public async Task<FileUploadResponse> UploadFileAsync(string fileName, Stream stream)
        {
            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_httpClient.BaseAddress, "file"),
                Headers = {{ "filename", fileName }},
                Content = new StreamContent(stream)
            };
            var response = await _httpClient.SendAsync(httpRequestMessage);
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FileUploadResponse>(data);
        }

        public async Task<ScanResultsResponse> GetResultsAsync(string dataId)
        {
            var response = await _httpClient.GetAsync("file/" + dataId);
            var data = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ScanResultsResponse>(data);
        }

        public async Task<ScanResultsResponse> ScanFileAndWaitAsync(string fileName, Stream stream)
        {
            var response = await UploadFileAsync(fileName, stream);
            return await Policy
                .HandleResult<ScanResultsResponse>(x => x.ScanResults.ProgressPercentage != 100)
                .WaitAndRetryAsync(10, retryAttempt => TimeSpan.FromSeconds(1))
                .ExecuteAsync(async x => await GetResultsAsync(response.DataId), CancellationToken.None);
        }
    }

    public class FileUploadResponse
    {
        [JsonProperty("data_id")]
        public string DataId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("in_queue")]
        public long InQueue { get; set; }

        [JsonProperty("queue_priority")]
        public string QueuePriority { get; set; }
    }

    public class ScanResultsResponse
    {
        [JsonProperty("file_id")]
        public string FileId { get; set; }

        [JsonProperty("data_id")]
        public string DataId { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("process_info")]
        public ProcessInfo ProcessInfo { get; set; }

        [JsonProperty("scan_results")]
        public ScanResults ScanResults { get; set; }

        [JsonProperty("file_info")]
        public FileInfo FileInfo { get; set; }

        [JsonProperty("top_threat")]
        public long TopThreat { get; set; }

        [JsonProperty("share_file")]
        public long ShareFile { get; set; }

        [JsonProperty("rest_version")]
        public long RestVersion { get; set; }

        [JsonProperty("votes")]
        public Votes Votes { get; set; }

        [JsonProperty("scan_result_history_length")]
        public long ScanResultHistoryLength { get; set; }
    }

    public class FileInfo
    {
        [JsonProperty("file_size")]
        public long FileSize { get; set; }

        [JsonProperty("upload_timestamp")]
        public DateTimeOffset? UploadTimestamp { get; set; }

        [JsonProperty("md5")]
        public string Md5 { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        [JsonProperty("file_type_category")]
        public string FileTypeCategory { get; set; }

        [JsonProperty("file_type_description")]
        public string FileTypeDescription { get; set; }

        [JsonProperty("file_type_extension")]
        public string FileTypeExtension { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }
    }

    public class ProcessInfo
    {
        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("progress_percentage")]
        public long ProgressPercentage { get; set; }

        [JsonProperty("profile")]
        public string Profile { get; set; }

        [JsonProperty("post_processing")]
        public PostProcessing PostProcessing { get; set; }

        [JsonProperty("file_type_skipped_scan")]
        public bool FileTypeSkippedScan { get; set; }

        [JsonProperty("blocked_reason")]
        public string BlockedReason { get; set; }
    }

    public class PostProcessing
    {
        [JsonProperty("copy_move_destination")]
        public string CopyMoveDestination { get; set; }

        [JsonProperty("converted_to")]
        public string ConvertedTo { get; set; }

        [JsonProperty("converted_destination")]
        public string ConvertedDestination { get; set; }

        [JsonProperty("actions_ran")]
        public string ActionsRan { get; set; }

        [JsonProperty("actions_failed")]
        public string ActionsFailed { get; set; }
    }

    public class ScanResults
    {
        [JsonProperty("scan_details")]
        public Dictionary<string, ScanDetail> ScanDetails { get; set; }

        [JsonProperty("rescan_available")]
        public bool RescanAvailable { get; set; }

        [JsonProperty("data_id")]
        public string DataId { get; set; }

        [JsonProperty("scan_all_result_i")]
        public long ScanAllResultI { get; set; }

        [JsonProperty("start_time")]
        public DateTimeOffset? StartTime { get; set; }

        [JsonProperty("total_time")]
        public long TotalTime { get; set; }

        [JsonProperty("total_avs")]
        public long TotalAvs { get; set; }

        [JsonProperty("total_detected_avs")]
        public long TotalDetectedAvs { get; set; }

        [JsonProperty("progress_percentage")]
        public long ProgressPercentage { get; set; }

        [JsonProperty("in_queue")]
        public long InQueue { get; set; }

        [JsonProperty("scan_all_result_a")]
        public string ScanAllResultA { get; set; }
    }

    public class ScanDetail
    {
        [JsonProperty("wait_time")]
        public long WaitTime { get; set; }

        [JsonProperty("threat_found")]
        public string ThreatFound { get; set; }

        [JsonProperty("scan_time")]
        public long ScanTime { get; set; }

        [JsonProperty("scan_result_i")]
        public long ScanResultI { get; set; }

        [JsonProperty("def_time")]
        public DateTimeOffset? DefTime { get; set; }
    }

    public class Votes
    {
        [JsonProperty("up")]
        public long Up { get; set; }

        [JsonProperty("down")]
        public long Down { get; set; }
    }
}
