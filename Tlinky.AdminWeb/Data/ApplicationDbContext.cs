using Microsoft.EntityFrameworkCore;
using Tlinky.AdminWeb.Models;
using AttendanceModel = Tlinky.AdminWeb.Models.Attendance;

namespace Tlinky.AdminWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ✅ DbSets
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<SystemSetting> Settings { get; set; }
        public DbSet<SystemUser> SystemUsers { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // ✅ Authentication user model
        public DbSet<User> Users { get; set; }

        // ✅ Global configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 Default: store all DateTimes as "timestamp without time zone"
            // This prevents UTC conversion errors in PostgreSQL
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                    {
                        // Exclude timezone-aware types
                        if (!(
                            (entityType.ClrType == typeof(Payment) && property.Name == nameof(Payment.DateUploaded)) ||
                            (entityType.ClrType == typeof(AttendanceModel) && property.Name == nameof(AttendanceModel.Date)) ||
                            (entityType.ClrType == typeof(Incident) && property.Name == nameof(Incident.Date))
                        ))
                        {
                            property.SetColumnType("timestamp without time zone");
                        }
                    }
                }
            }

            // ✅ Explicit timezone-aware columns (keep as is)
            modelBuilder.Entity<Payment>()
                .Property(p => p.DateUploaded)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Attendance>()
                .Property(a => a.Date)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Incident>()
                .Property(i => i.Date)
                .HasColumnType("timestamp with time zone");

            // ✅ Default Admin seed
            var adminEmail = "admin@tlinky.org";
            var adminPassword = BCrypt.Net.BCrypt.HashPassword("1234");
            modelBuilder.Entity<User>().HasData(new User
            {
                UserId = 1,
                Email = adminEmail,
                PasswordHash = adminPassword,
                Role = "Admin"
            });

            // ✅ Cascade delete: Class → Children
            modelBuilder.Entity<Class>()
                .HasMany(c => c.Children)
                .WithOne(ch => ch.Class)
                .HasForeignKey(ch => ch.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Cascade delete: Parent → Children
            modelBuilder.Entity<Parent>()
                .HasMany(p => p.Children)
                .WithOne(ch => ch.Parent)
                .HasForeignKey(ch => ch.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Cascade delete: Child → Attendance / Payments / Incidents
            modelBuilder.Entity<Child>()
                .HasMany<Attendance>()
                .WithOne(a => a.Child)
                .HasForeignKey(a => a.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Child>()
                .HasMany<Payment>()
                .WithOne(p => p.Child)
                .HasForeignKey(p => p.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Child>()
                .HasMany<Incident>()
                .WithOne(i => i.Child)
                .HasForeignKey(i => i.ChildId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
