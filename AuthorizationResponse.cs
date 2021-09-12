using System.Text.Json.Serialization;

namespace BackblazeB2Info
{
    public class AuthorizationResponse
    {
        [JsonPropertyName("accountId")]
        public string AccountId { get; set; } = default!;

        [JsonPropertyName("apiUrl")]
        public string ApiUrl { get; set; } = default!;

        [JsonPropertyName("authorizationToken")]
        public string AuthorizationToken { get; set; } = default!;

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; } = default!;

        [JsonPropertyName("s3ApiUrl")]
        public string S3ApiUrl { get; set; } = default!;

        public override string ToString()
            => $"AccountId: {AccountId}\nApiUrl: {ApiUrl}\nAuthToken: {AuthorizationToken}\nDownload URL: {DownloadUrl}\nS3ApiUrl: {S3ApiUrl}";
    }
}
