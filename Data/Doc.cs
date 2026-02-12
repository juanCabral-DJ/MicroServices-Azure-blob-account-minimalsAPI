namespace minimalAPI.Data
{
    public class Doc
    {
        public Guid id { get; set; }
        public Guid tenant_id { get; set; }
        public string doc_tyoe_code { get; set; }
        public string file_name { get; set; }
        public string content_type { get; set; }
        public long content_length { get; set; }
        public string checksum_sha256 { get; set; }
        public string storage_provider { get; set; }
        public string storage_key { get; set; }
        public long row_status_id { get; set; }
        public DateTime create_at { get; set; }
        public DateTime update_at { get; set; }
    }
}
