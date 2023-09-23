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
    }
}
