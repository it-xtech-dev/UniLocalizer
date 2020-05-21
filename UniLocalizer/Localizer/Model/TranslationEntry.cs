namespace UniLocalizer.Localizer.Entity
{
    using Newtonsoft.Json;
    using System.ComponentModel.DataAnnotations.Schema;

    public abstract class TranslationEntry<T> where T : TranslationEntry<T>, new()
    {
        [Column(TypeName = "varchar(5)")]
        [JsonProperty(PropertyName = "culture")]
        public string CultureName { get; set; }

    }
}
