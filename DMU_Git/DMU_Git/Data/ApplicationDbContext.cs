using DMU_Git.Models;
using Microsoft.EntityFrameworkCore;

namespace DMU_Git.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
                
        }
        public DbSet<EntityListMetadataModel> EntityListMetadataModels { get; set; }

        public DbSet<EntityColumnListMetadataModel> EntityColumnListMetadataModels { get; set; }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<EntityListMetadataModel>()
        //        .HasMany(e => e.EntityColumnListMetadata)
        //        .WithOne()
        //        .HasForeignKey(e => e.EntityId);

        //    // Other configuration...

        //    base.OnModelCreating(modelBuilder);
        //}


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityListMetadataModel>()
                .HasMany(e => e.EntityColumnListMetadata)
                .WithOne()
                .HasForeignKey(e => e.EntityId);

            modelBuilder.Entity<EntityColumnListMetadataModel>()
                .HasOne(e => e.EntityList)
                .WithMany(l => l.EntityColumns)
                .HasForeignKey(e => e.EntityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Additional configurations or constraints can be added here if needed

            // Call the base method at the end
            base.OnModelCreating(modelBuilder);
        }
    }
}
