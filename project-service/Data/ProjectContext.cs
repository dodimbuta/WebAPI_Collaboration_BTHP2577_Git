using Microsoft.EntityFrameworkCore;
using project_service.Models.Entities;

namespace project_service.Data
{
    public class ProjectContext : DbContext
    {
        public ProjectContext(DbContextOptions<ProjectContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Project>()
            //    .HasKey(p => p.Id);
            //modelBuilder.Entity<Project>()
            //    .HasOne<User>() 
            //    .WithMany()     
            //    .HasForeignKey(p => p.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

        }

    }

}
