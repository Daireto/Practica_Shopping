using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shopping.Data.Entities;

namespace Shopping.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        //-------------------- Entities ----------------------
        public DbSet<Category> Categories { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<City> Cities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Category>().HasIndex(c => c.Name).IsUnique(); //Category index
            modelBuilder.Entity<Country>().HasIndex(c => c.Name).IsUnique(); //Country index
            modelBuilder.Entity<State>().HasIndex("Name", "CountryId").IsUnique(); //State index
            modelBuilder.Entity<City>().HasIndex("Name", "StateId").IsUnique(); //City index
        }
    }
}
