using Microsoft.EntityFrameworkCore;
using QuanTraSua.Helpers;

namespace QuanTraSua;

public class QL_TraSuaContext : DbContext
{
    public DbSet<DanhMucMon> DanhMucMons => Set<DanhMucMon>();
    public DbSet<MonUong> MonUongs => Set<MonUong>();
    public DbSet<HoaDon> HoaDons => Set<HoaDon>();
    public DbSet<ChiTietHoaDon> ChiTietHoaDons => Set<ChiTietHoaDon>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(AppConfig.Load().ConnectionString);

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<DanhMucMon>(e =>
        {
            e.ToTable("DanhMucMon");
            e.HasKey(x => x.MaDM);
            e.Property(x => x.TenDM).HasMaxLength(50);
        });

        mb.Entity<MonUong>(e =>
        {
            e.ToTable("MonUong");
            e.HasKey(x => x.MaMon);
            e.Property(x => x.TenMon).HasMaxLength(100);
            e.Property(x => x.GiaBan).HasColumnType("decimal(18,0)");
            e.HasOne(x => x.DanhMuc).WithMany(d => d.MonUongs).HasForeignKey(x => x.MaDM);
        });

        mb.Entity<HoaDon>(e =>
        {
            e.ToTable("HoaDon");
            e.HasKey(x => x.MaHD);
            e.Property(x => x.TongTien).HasColumnType("decimal(18,0)");
            e.Property(x => x.HinhThucTT).HasMaxLength(20);
        });

        mb.Entity<ChiTietHoaDon>(e =>
        {
            e.ToTable("ChiTietHoaDon");
            e.HasKey(x => new { x.MaHD, x.MaMon });
            e.Property(x => x.DonGia).HasColumnType("decimal(18,0)");
            e.HasOne(x => x.HoaDon).WithMany(h => h.ChiTietHoaDons).HasForeignKey(x => x.MaHD);
            e.HasOne(x => x.MonUong).WithMany().HasForeignKey(x => x.MaMon);
        });
    }
}
