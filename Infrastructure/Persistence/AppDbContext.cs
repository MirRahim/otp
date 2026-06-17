using Microsoft.EntityFrameworkCore;
using OtpSystem.Domain.Entities;

namespace OtpSystem.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<OtpRecord> OtpRecords => Set<OtpRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Tenant ────────────────────────────────────────────────
        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("Tenants");
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(100);
            e.Property(t => t.ApiKey).IsRequired().HasMaxLength(256);
            e.Property(t => t.WebhookUrl).HasMaxLength(500);

            // fast lookup by API key
            e.HasIndex(t => t.ApiKey).IsUnique();
        });

        // ── OtpRecord ─────────────────────────────────────────────
        modelBuilder.Entity<OtpRecord>(e =>
        {
            e.ToTable("OtpRecords");
            e.HasKey(o => o.Id);
            e.Property(o => o.Phone).IsRequired().HasMaxLength(20);
            e.Property(o => o.Code).IsRequired().HasMaxLength(256);
            e.Property(o => o.Status).HasConversion<string>();

            // FK
            e.HasOne<Tenant>()
             .WithMany()
             .HasForeignKey(o => o.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            // fast lookup: tenant + phone + pending
            e.HasIndex(o => new { o.TenantId, o.Phone, o.Status });
        });
    }
}