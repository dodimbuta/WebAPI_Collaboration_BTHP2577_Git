using Microsoft.EntityFrameworkCore;
using user_service.Models.Entities;



namespace user_service.Data
{
    public class UserContext: DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        //public DbSet<Project> Projects { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    //base.OnModelCreating(modelBuilder);
        //    //modelBuilder.Entity<Project>()
        //}

    }
}
