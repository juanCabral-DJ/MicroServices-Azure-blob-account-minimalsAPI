namespace minimalAPI.Data
{
    public class DocLinks
    {
        public Guid id { get; set; }
        public Guid tenant_id { get; set; }
        public Guid doc_id { get; set; }
        public string entity_type { get; set; }
        public Guid entity_id { get; set; }
        public string role { get; set; }
        public string notes { get; set; }
        public long row_status_id { get; set; }
        public DateTime create_at { get; set; }
        public DateTime update_at { get; set; }
    }
}
