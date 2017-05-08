using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    [ConfigurationCollection(typeof(ReplacementElement), AddItemName = "replace")]
    public class ReplacementElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ReplacementElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ReplacementElement)element).OriginValue;
        }
    }
}