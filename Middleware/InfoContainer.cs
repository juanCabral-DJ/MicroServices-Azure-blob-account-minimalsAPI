namespace minimalAPI.Middleware
{
    public class InfoContainer
    {
        public Guid TenantId { get; set; }
        public string? entitytype { get; set; } = string.Empty;
    }
}
