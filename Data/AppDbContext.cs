using Microsoft.EntityFrameworkCore;

namespace minimalAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Doc> doc { get; set; }
        public DbSet<DocLinks> doc_link { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doc>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.doc_tyoe_code).HasMaxLength(50);
                entity.Property(e => e.file_name).HasMaxLength(255);
                entity.Property(e => e.content_type).HasMaxLength(100);
                entity.Property(e => e.checksum_sha256).HasMaxLength(64);
                entity.Property(e => e.storage_provider).HasMaxLength(50);
                entity.Property(e => e.storage_key).HasMaxLength(255);
            });
            modelBuilder.Entity<DocLinks>(entity =>
            {
                entity.HasKey(e => e.id);
                entity.Property(e => e.entity_type).HasMaxLength(50);
                entity.Property(e => e.role).HasMaxLength(50);
                entity.Property(e => e.notes).HasMaxLength(500);
            });
        }
    }
}
