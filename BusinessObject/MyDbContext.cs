using Microsoft.EntityFrameworkCore;
using BusinessObject.Models;

namespace BusinessObject
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() { }
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<CorrectionRequest> CorrectionRequests { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(local);Database=AttendanceSystem;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Xóa RefreshToken nếu User bị xóa
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // AttendanceRecord relationship
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.Attendances)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // CorrectionRequest relationship
            modelBuilder.Entity<CorrectionRequest>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
