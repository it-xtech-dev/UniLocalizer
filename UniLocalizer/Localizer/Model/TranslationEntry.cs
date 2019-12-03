using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniLocalizer.Localizer.Entity
{
    public abstract class TranslationEntry<T> where T : TranslationEntry<T>, new()
    {
        [Column(TypeName = "varchar(5)")]
        public string CultureName { get; set; }

    }
}
