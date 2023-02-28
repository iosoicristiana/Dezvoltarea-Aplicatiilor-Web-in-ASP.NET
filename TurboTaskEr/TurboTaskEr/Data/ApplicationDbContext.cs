using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TurboTaskEr.Models;
using Task = TurboTaskEr.Models.Task;

namespace TurboTaskEr.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Task> Tasks { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Status> Statuses { get; set; }

        public DbSet<Team> Teams { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);

            // definire primary key compus
            modelBuilder.Entity<Team>()
                .HasKey(ab => new { ab.Id, ab.UserId, ab.ProjectId});


            // definire relatii cu modelele Bookmark si Article (FK)
            modelBuilder.Entity<Team>()
                .HasOne(ab => ab.User)
                .WithMany(ab => ab.Teams)
                .HasForeignKey(ab => ab.UserId);

            modelBuilder.Entity<Team>()
                .HasOne(ab => ab.Project)
                .WithMany(ab => ab.Teams)
                .HasForeignKey(ab => ab.ProjectId);
        }

    }
}