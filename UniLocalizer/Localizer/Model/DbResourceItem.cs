using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace UniLocalizer.Localizer.Model
{
    [Table("LocalizerResourceItem")]
    public class DbResourceItem
    {
        public int Id { get; set; }

        [Column(TypeName = "varchar(5)")]
        [Required]
        public string CultureName { get; set; }

        [Column(TypeName = "varchar(200)")]
        [Required]
        public string LocationKey { get; set; }

        [Column(TypeName = "varchar(200)")]
        [Required]
        public string ResourceKey { get; set; }

        [Column(TypeName = "varchar(300)")]
        [Required]
        public string Value { get; set; }

        public CultureInfo Culture
        {
            get
            {
                return new CultureInfo(this.CultureName);
            }
        }

        public string GeneralKey
        {
            get
            {
                return this.Culture + ":" + this.LocationKey + ":" + this.ResourceKey;
            }
        }
    }
}
