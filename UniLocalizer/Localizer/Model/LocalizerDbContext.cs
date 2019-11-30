using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UniLocalizer.Localizer.Model
{
    public class LocalizerDbContext : DbContext
    {
        public LocalizerDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<DbResourceItem> ResourceItems { get; set; }
    }
}
