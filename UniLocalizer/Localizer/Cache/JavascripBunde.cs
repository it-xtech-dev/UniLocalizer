using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UniLocalizer.Extensions;

namespace UniLocalizer.Localizer.Cache
{
    public class JavascriptCacheEntry
    {
        private string content;

        public JavascriptCacheEntry(List<string> locationKeys, string content)
        {
            this.Content = content;
            this.LocationKeys = locationKeys;
            this.CacheEntryHash = string.Join("_", locationKeys);
        }

        public string CacheEntryHash { get; private set; }
        
        public List<string> LocationKeys { get; private set; }

        public string Content { 
            get
            {
                return this.content;
            }
            private set
            {
                if (this.content != value)
                {
                    this.ContentHash = value.GetHashString();
                    this.content = value;
                }
            }
        }

        public string ContentHash { get; private set; }
    }
}
