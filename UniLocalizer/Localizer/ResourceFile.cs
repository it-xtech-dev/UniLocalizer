﻿using System;
using System.Globalization;
using UniLocalizer.Extensions;

namespace UniLocalizer
{
    /// <summary>
    /// Provides resource file class that represents single json file.
    /// </summary>
    public class ResourceFile
    {
        private string contentHash;
        private string content;


        /// <summary>
        /// Creates new instance of resource file.
        /// </summary>
        /// <param name="path">The resouce file path</param>
        /// <param name="key">The resouce file location key</param>
        /// <param name="content">The resouce file content</param>
        public ResourceFile(int index, string key, string content)
        {
            var keySplit = key.Split(":");
            var cultureName = keySplit[0];
            var location = keySplit[1];

            this.Index = index;
            this.KeyHashed = ResourceFile.GetKeyHash(key);
            this.Key = key;
            this.content = content;
            this.IsSynced = true;
            this.Culture = new CultureInfo(cultureName);
            this.LocationKey = location;
            this.RelativePath = GetRelativePath(key);
        }

        /// <summary>
        /// Resource file sync required event. Raised when accesing content related props and IsSynced flag is false (enabled lazy sync between resource items and resource files).
        /// </summary>
        public event EventHandler<SyncRequiredArgs> OnSyncRequired;

        /// <summary>
        /// Gets resource file unique index.
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Gets resouce file path;
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Gets hashed resouce file location key.
        /// </summary>
        public string KeyHashed { get; private set; }

        /// <summary>
        /// Gets resouce file location key.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets hashed resouce file content.
        /// </summary>
        public string Content { 
            get
            {
                if (!this.IsSynced) this.OnSyncRequired?.Invoke(this, new SyncRequiredArgs(this));
                return this.content;
            }
            set
            {
                if (this.content != value)
                {
                    this.contentHash = content.GetHashString();
                    this.content = value;
                }
            } 
        }

        public CultureInfo Culture
        {
            get; private set;
        }

        public string LocationKey
        {
            get; private set;
        }

        /// <summary>
        /// Gets content hashcode.
        /// </summary>
        public string ContentHash { 
            get {
                if (!this.IsSynced) this.OnSyncRequired?.Invoke(this, new SyncRequiredArgs(this));
                return this.contentHash; 
            } 
        }

        /// <summary>
        /// Determines whether file is synced with its resource items.
        /// </summary>
        public bool IsSynced
        {
            get; set;
        }

        public static string GetKeyHash(string key)
        {
            return key.GetHashString();
        }

        public string GetRelativePath(string key)
        {
            var pathAndFileCore = this.LocationKey.Replace(".",@"\");
            var path = pathAndFileCore + "." + this.Culture.Name + ".json";

            return path;
        }

        /// <summary>
        /// The resource file sync required event args.
        /// </summary>
        public class SyncRequiredArgs : EventArgs
        {
            public ResourceFile File;

            public SyncRequiredArgs(ResourceFile file)
            {
                File = file;
            }
        }
    }
}
