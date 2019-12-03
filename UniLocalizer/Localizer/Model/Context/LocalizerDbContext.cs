using Microsoft.EntityFrameworkCore;

namespace UniLocalizer.Localizer.Model.Context
{
    public class LocalizerDbContext : DbContext
    {
        public LocalizerDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DbResourceItem> ResourceItems { get; set; }
    }
}
