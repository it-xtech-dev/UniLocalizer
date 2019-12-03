using Microsoft.EntityFrameworkCore;

namespace UniLocalizer.Demo.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LocalizerResourceItem> ResourceItems { get; set; }

        public DbSet<Book> Books { get; set; }

        public DbSet<BookTranslation> BookTranslations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Book>(e => e
            //.HasMany(t => t.Translations)
            //.WithOne(bt => bt.Book));

            modelBuilder.Entity<BookTranslation>(e =>
                e.HasKey(t => new { t.CultureName, t.BookId })
            );

            //modelBuilder.Entity<Book>(entity =>
            //{
            //    entity
            //        .HasMany(x => x.Translations)
            //        .WithOne(x => x.Book);
            //});
        }
    }
}
