namespace BackblazeB2Info.ApplicationConfiguration
{
    public class StorageAccount
    {
        public string ApplicationKeyId { get; set; } = default!;
        public string ApplicationKey { get; set; } = default!;
        public Bucket[] Buckets { get; set; } = default!;
    }
}
