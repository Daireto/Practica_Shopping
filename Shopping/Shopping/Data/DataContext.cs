using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;

namespace Shopping.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        //-------------------- Entities ----------------------
        public DbSet<Country>? Country { get; set; }
        public DbSet<Category>? Category { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique(); //Country index
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique(); //Category index
        }
    }
}
