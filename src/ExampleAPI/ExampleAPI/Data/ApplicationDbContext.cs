using ExampleAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ExampleAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured) optionsBuilder.UseMySql("server=localhost;port=3306;user=root;database=api_demo_3945", new MySqlServerVersion(new Version(10, 4, 24)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name)
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_general_ci");

                entity.Property(e => e.Description)
                    .HasCharSet("utf8mb4")
                    .UseCollation("utf8mb4_general_ci");


                entity.HasData(
                    new Product[]
                    {
                        new Product() {ID = 1, Name="Milk", Description = "Moo juice."},
                        new Product()  {ID = 2, Name="Cereal", Description = "Crunchy breakfast snack."},
                        new Product()  {ID = 3, Name="Broccoli", Description = "More flowery kale."}
                    });
            });
        }
    }
}
