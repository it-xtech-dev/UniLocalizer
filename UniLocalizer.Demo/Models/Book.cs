using System.ComponentModel.DataAnnotations.Schema;
using UniLocalizer.Localizer.Entity;

namespace UniLocalizer.Demo.Models
{
    public class Book: LocalizableEntity<BookTranslation>
    {

        public int BookId { get; set; }

        public string OriginalTitle { get; set; }

        [NotMapped]
        public string TranslatedTitle
        {
            get { return this.Translated?.Title; }
            set { this.TranslatedOrNew.Title = value; }
        }

        public string Author { get; set; }
    }
}
