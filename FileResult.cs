using System.Text.Json.Serialization;

namespace BackblazeB2Info
{
    public class FileCollectionResult
    {
        [JsonPropertyName("files")]
        public FileResult[] Files { get; set; } = default!;

        [JsonPropertyName("nextFileName")]
        public string? NextFileName { get; set; }

    }

    public class FileResult
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = default!;

        [JsonPropertyName("contentLength")]
        public long Size { get; set; }
    }
}
