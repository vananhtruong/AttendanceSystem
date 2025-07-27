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
        public DbSet<WorkShift> WorkShifts { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<SalaryRecord> salaryRecords { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(local);Database=AttendanceSystem;Trusted_Connection=True;TrustServerCertificate=True");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Fix decimal warnings
            modelBuilder.Entity<SalaryRecord>(entity =>
            {
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.TotalHoursWorked).HasPrecision(5, 2);
                entity.Property(e => e.OvertimeHours).HasPrecision(5, 2);
            });

            // Cascade delete khi xóa User => xóa token
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(ar => ar.User)
                .WithMany()
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CorrectionRequest>()
                .HasOne(cr => cr.User)
                .WithMany()
                .HasForeignKey(cr => cr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification: Cascade delete khi xóa User => xóa Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
