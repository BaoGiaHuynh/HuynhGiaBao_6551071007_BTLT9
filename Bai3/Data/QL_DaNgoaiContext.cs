using KeHoachDaNgoai.Helpers;
using Microsoft.EntityFrameworkCore;

namespace KeHoachDaNgoai;

public class QL_DaNgoaiContext : DbContext
{
    public DbSet<ChuyenDi> ChuyenDis => Set<ChuyenDi>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlServer(AppSettings.ConnectionString);

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<ChuyenDi>(e =>
        {
            e.ToTable("ChuyenDi");
            e.HasKey(x => x.MaChuyen);
            e.Property(x => x.TenChuyen).HasMaxLength(100);
            e.Property(x => x.DiaDiem).HasMaxLength(100);
            e.Property(x => x.NgayDi).HasColumnType("date");
            e.Property(x => x.NganSach).HasColumnType("decimal(18,0)");
            e.Property(x => x.PhuongTien).HasMaxLength(30);
            e.Property(x => x.GhiChu).HasMaxLength(200);
        });
    }
}
