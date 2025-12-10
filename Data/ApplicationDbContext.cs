using Microsoft.EntityFrameworkCore;
using PRAKTOSWEBAPI.Models;

namespace PRAKTOSWEBAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-one: User - Applicant (один User может иметь одного Applicant)
            modelBuilder.Entity<Applicant>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one: Applicant - Application
            modelBuilder.Entity<Application>()
                .HasOne(app => app.Applicant)
                .WithOne()  // Нет обратной навигации, если не нужна
                .HasForeignKey<Application>(app => app.Id);

            // Seed данных: Добавим несколько специальностей
            modelBuilder.Entity<Specialty>().HasData(
                new Specialty { Id = 1, Name = "Программирование" },
                new Specialty { Id = 2, Name = "Дизайн" },
                new Specialty { Id = 3, Name = "Менеджмент" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}