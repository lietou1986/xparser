using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    [ConfigurationCollection(typeof(ImageSizeElement), AddItemName = "size")]
    public class ImageCompressorElementCollection : ConfigurationElementCollection
    {
        public new ImageSizeElement this[string name]
        {
            get
            {
                return (ImageSizeElement)BaseGet(name);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ImageSizeElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ImageSizeElement)element).Name;
        }
    }
}