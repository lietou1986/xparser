using System;
using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    [ConfigurationCollection(typeof(DocumentExtractorElement), AddItemName = "documentExtractor")]
    public class DocumentExtractorElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DocumentExtractorElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DocumentExtractorElement)element).Name;
        }

        internal DocumentExtractorElement GetJobBoard(string boardName)
        {
            if (boardName == null)
            {
                throw new ArgumentNullException("documentExtractor");
            }
            return BaseGet(boardName) as DocumentExtractorElement;
        }
    }
}