using Microsoft.EntityFrameworkCore;
using QuanLyNhaTro.Models;

namespace QuanLyNhaTro.Data;

// Giống DbContext do scaffold sinh ra, nhưng chuỗi kết nối được truyền từ appsettings.json
public class QlNhaTroContext : DbContext
{
    private readonly string _connectionString;

    public QlNhaTroContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public virtual DbSet<PhongTro> PhongTros { get; set; }
    public virtual DbSet<KhachThue> KhachThues { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhongTro>(e =>
        {
            e.ToTable("PhongTro");
            e.HasKey(x => x.MaPhong);
            e.Property(x => x.TenPhong).HasMaxLength(20);
            e.Property(x => x.GiaThue).HasColumnType("decimal(18, 0)");
            e.Property(x => x.TinhTrang).HasMaxLength(20);
        });

        modelBuilder.Entity<KhachThue>(e =>
        {
            e.ToTable("KhachThue");
            e.HasKey(x => x.MaKhach);
            e.Property(x => x.HoTen).HasMaxLength(100);
            e.Property(x => x.GioiTinh).HasMaxLength(5);
            e.Property(x => x.NgaySinh).HasColumnType("date");
            e.Property(x => x.SoDienThoai).HasMaxLength(15);
            e.Property(x => x.TenTinh).HasMaxLength(100);
            e.Property(x => x.TenPhuongXa).HasMaxLength(100);

            e.HasOne(x => x.MaPhongNavigation)
             .WithMany(p => p.KhachThues)
             .HasForeignKey(x => x.MaPhong)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK_KhachThue_PhongTro");
        });
    }
}
