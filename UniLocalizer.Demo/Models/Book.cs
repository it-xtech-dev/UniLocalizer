using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
            get { return this.Translated.Title; }
            set { this.Translated.Title = value; }
        }

        public string Author { get; set; }
    }
}
