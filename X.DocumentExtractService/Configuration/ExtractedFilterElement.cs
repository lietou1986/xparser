using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    internal class ExtractedFilterElement : ConfigurationElement
    {
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