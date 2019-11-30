using System;
using System.Globalization;


namespace UniLocalizer
{
    /// <summary>
    /// Provides resource item class that represents single locale entry for culture, file location and key.
    /// </summary>
    public class ResourceItem
    {
        private string _value;

        /// <summary>
        /// Initializes new instance of resource item
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <param name="resourceFilePath">The file path where resource key is located</param>
        /// <param name="resourceLocationKey">The resouce file location key: .data.fileName </param>
        /// <param name="key">The resource key.</param>
        /// <param name="value">The resouce value</param>
        public ResourceItem(CultureInfo culture, string resourceLocationKey, string key, string value, ResourceFile file)
        {
            this.Culture = culture;
            //this.ResourceFilePath = resourceFilePath;
            this.LocationKey = resourceLocationKey;
            this.Key = key;
            this.File = file;
            // do not trigger modification when resource item is initially created.
            this._value = value;
            this.IsModified = false;
        }

        /// <summary>
        /// Resource item modfication event. Raised when resource item value is modified.
        /// </summary>
        public event EventHandler<ModifiedArgs> OnModification;

        /// <summary>
        /// The resource file related with resource item.
        /// </summary>
        public ResourceFile File
        {
            get; private set;
        }

        /// <summary>
        /// Gets resouce item culture
        /// </summary>
        public CultureInfo Culture
        {
            get; private set;
        }

        /// <summary>
        /// Gets resource item general key
        /// </summary>
        public string GeneralKey
        {
            get
            {
                return GetGeneralKey(this.Culture, this.LocationKey, this.Key);
            }
        }

        /// <summary>
        /// Gets resource file location key.
        /// </summary>
        public string LocationKey
        {
            get; private set;
        }

        /// <summary>
        /// Gets resource item key.
        /// </summary>
        public string Key
        {
            get; private set;
        }

        /// <summary>
        /// Gets or sets resource item value
        /// </summary>
        public string Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (this._value != value)
                {
                    this.IsModified = true;
                    this.File.IsSynced = false;
                    OnModification?.Invoke(this, new ModifiedArgs(value, this._value));
                }
                this._value = value;
            }
        }

        /// <summary>
        /// Gets or sets value inidicating whether resoure value was modified since load or after saving.
        /// </summary>
        public bool IsModified
        {
            get; set;
        }

        public static string GetGeneralKey(CultureInfo culture, string locationKey, string resourceKey)
        {
            return culture.Name + ":" + locationKey + ":" + resourceKey;
        }

        public static string GetGeneralKey(string cultureName, string locationKey, string resourceKey)
        {
            return cultureName + ":" + locationKey + ":" + resourceKey;
        }

        public static string GetFileKey(CultureInfo culture, string locationKey)
        {
            return culture.Name + ":" + locationKey;
        }

        public static string GetFileKey(string cultureName, string locationKey)
        {
            return cultureName + ":" + locationKey;
        }


        /// <summary>
        /// The resource modification event args.
        /// </summary>
        public class ModifiedArgs : EventArgs
        {
            public string OldValue { get; }
            public string Value { get; }

            public ModifiedArgs(string value, string oldValue)
            {
                Value = value;
                OldValue = oldValue;
            }
        }
    }
}
