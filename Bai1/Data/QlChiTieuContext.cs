using Microsoft.EntityFrameworkCore;
using SoChiTieu.Models;

namespace SoChiTieu.Data;

// Giống DbContext do scaffold sinh ra, nhưng chuỗi kết nối được truyền từ appsettings.json
public class QlChiTieuContext : DbContext
{
    private readonly string _connectionString;

    public QlChiTieuContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual DbSet<ChiTieu> ChiTieus { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChiTieu>(entity =>
        {
            entity.ToTable("ChiTieu");
            entity.HasKey(e => e.MaCT);
            entity.Property(e => e.NgayChi).HasColumnType("date");
            entity.Property(e => e.NoiDung).HasMaxLength(100);
            entity.Property(e => e.LoaiChi).HasMaxLength(30);
            entity.Property(e => e.SoTien).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.GhiChu).HasMaxLength(200);
        });
    }
}
