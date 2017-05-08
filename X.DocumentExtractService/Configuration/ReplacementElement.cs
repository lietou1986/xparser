using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    internal class ReplacementElement : ConfigurationElement
    {
        [ConfigurationProperty("origin")]
        internal string OriginValue
        {
            get
            {
                return (string)base["origin"];
            }
        }

        [ConfigurationProperty("replace")]
        internal string ReaplaceValue
        {
            get
            {
                return (string)base["replace"];
            }
        }
    }
}