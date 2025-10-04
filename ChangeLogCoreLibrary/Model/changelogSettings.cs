using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChangeLogCoreLibrary.Model
{
    public class changelogSettings : ConfigurationSection
    {
        /// <summary>
        /// The name of this section in the app.config.
        /// </summary>
        public const string SectionName = "changelogSettings";

        private const string CollectionName = "settings";

        [ConfigurationProperty(CollectionName)]
        [ConfigurationCollection(typeof(changelogSettingsCollection), AddItemName = "add")]
        public changelogSettingsCollection ChangeLogSettings 
        { 
            get 
            { 
                return (changelogSettingsCollection)base[CollectionName]; 
            } 
        }
    }

    public class changelogSettingsCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new changelogSettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((changelogSettingElement)element).key;
        }

        public changelogSettingElement this[string key]
        {
            get { return (changelogSettingElement)BaseGet(key); }
        }
    }

    public class changelogSettingElement : ConfigurationElement
    {
        [ConfigurationProperty("key", IsRequired = true)]
        public string key
        {
            get { return (string)this["key"]; }
            set { this["key"] = value; }
        }

        [ConfigurationProperty("value")]
        public string value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }
    }
}