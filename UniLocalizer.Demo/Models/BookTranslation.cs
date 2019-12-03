using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UniLocalizer.Localizer.Entity;
using LocalizerModel = UniLocalizer.Localizer.Model;

namespace UniLocalizer.Demo.Models
{
    [Table("BookTranslation")]
    public class BookTranslation : TranslationEntry<BookTranslation>
    {
        // this will introduce foreign key for book

        public int BookId { get; set; }

        public string Title { get; set; }
    }
}
