using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    public class DocumentExtractorElement : ConfigurationElement
    {
        [ConfigurationProperty("documentExtensions")]
        internal NameValueConfigurationCollection DocumentExtensions
        {
            get
            {
                return (NameValueConfigurationCollection)base["documentExtensions"];
            }
        }

        [ConfigurationProperty("extractors")]
        internal NameValueConfigurationCollection Extractors
        {
            get
            {
                return (NameValueConfigurationCollection)base["extractors"];
            }
        }

        [ConfigurationProperty("name")]
        internal string Name
        {
            get
            {
                return (string)base["name"];
            }
        }
    }
}