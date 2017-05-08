using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    [ConfigurationCollection(typeof(ExtractedFilterElement), AddItemName = "filter")]
    internal class ExtractedFilterElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ExtractedFilterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExtractedFilterElement)element).Name;
        }
    }
}