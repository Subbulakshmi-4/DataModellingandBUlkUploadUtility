using DMU_Git.Models;
using Microsoft.EntityFrameworkCore;



namespace DMU_Git.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<EntityListMetadataModel> EntityListMetadataModels { get; set; }



        public DbSet<EntityColumnListMetadataModel> EntityColumnListMetadataModels { get; set; }



        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<EntityListMetadataModel>()
        //        .HasMany(e => e.EntityColumnListMetadata) // A EntityListMetadataModel can have many EntityColumnListMetadataModels
        //        .WithOne(column => column.EntityList) // EntityColumnListMetadataModel has one EntityListMetadataModel
        //        .HasForeignKey(column => column.EntityId);

        //    // Other configuration...

        //    base.OnModelCreating(modelBuilder);
        //}
    }
}