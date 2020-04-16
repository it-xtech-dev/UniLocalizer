namespace UniLocalizer.Localizer.Entity
{
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class TranslationEntry<T> where T : TranslationEntry<T>, new()
    {
        [Column(TypeName = "varchar(5)")]
        public string CultureName { get; set; }

    }
}
