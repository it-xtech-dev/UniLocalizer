using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace UniLocalizer.Localizer.Entity
{
    public abstract class LocalizableEntity<T> where T : TranslationEntry<T>, new()
    {

        public virtual TranslationCollection<T> Translations { 
            get; 
            set; 
        } = new TranslationCollection<T>();

        /// <summary>
        /// Gets translation entry for current language. Returns null if translation doesn't exists.
        /// </summary>
        [NotMapped]
        public virtual T Translated {
            get
            {
                return this.Translations[CultureInfo.CurrentCulture.Name];
            }
        }

        /// <summary>
        /// Gets translation entry for current language. Creates entry if translation doesn't exists.
        /// WARNING: do not use attribute (TranslatedOrNew { get; }) due to creating "empty" translations in scenario - object used as model in POST, GET, ... requests.
        /// </summary>
        public virtual T GetTranslatedOrNew()
        {
            return this.Translations.GetOrCreate(CultureInfo.CurrentCulture.Name);
        }
    }
}

/// <summary>
/// Merges translations between current instance and database existing.
/// </summary>
/// <param name = "dbTranslations" ></ param >
//public void MergeTranslations<K>(EntityEntry<K> entityEntry) where K : TranslationBase<T>
//{
//    var baseTranslations = this.Translations.ToList();

//    this.Translations.Clear();

//    entityEntry.Context.Attach(entityEntry.Entity);


//    var dbTranslations = entityEntry.Entity.Translations;
//    foreach (var t in baseTranslations)
//    {
//        var baseT = t;
//        var mergedT = dbTranslations[t.CultureName];

//        baseT.TranslationGuid = mergedT.TranslationGuid;
//        entityEntry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
//    }

//    // remove items that are already in db
//    this.Translations.Clear();

//    baseTranslations.ForEach(t => {
//        this.Translations.Add(t);
//        entityEntry.Context.Entry(t).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
//    });

//    //foreach (var t in this.Translations)
//    //{
//    //    var baseT = t;
//    //    var mergedT = dbTranslations[t.CultureName];

//    //    baseT.TranslationGuid = mergedT.TranslationGuid;
//    //}
//}