using System.Configuration;

namespace X.DocumentExtractService.Configuration
{
    public class DocumentExtractorSection : ConfigurationSection
    {
        private static DocumentExtractorSection _currentSection;

        internal static DocumentExtractorSection Current
        {
            get
            {
                if (_currentSection == null)
                {
                    lock (typeof(DocumentExtractorSection))
                    {
                        if (_currentSection == null)
                        {
                            DocumentExtractorSection section = ConfigurationManager.GetSection("documentExtractService") as DocumentExtractorSection;
                            if (section == null)
                            {
                                throw new ConfigurationErrorsException("配置文件错误");
                            }
                            _currentSection = section;
                        }
                    }
                }
                return _currentSection;
            }
        }

        [ConfigurationProperty("documentExtractors", IsRequired = true)]
        internal DocumentExtractorElementCollection DocumentExtractors
        {
            get
            {
                return (DocumentExtractorElementCollection)base["documentExtractors"];
            }
        }

        [ConfigurationProperty("extractedFilters", IsRequired = false)]
        internal ExtractedFilterElementCollection ExtractedFilters
        {
            get
            {
                return (ExtractedFilterElementCollection)base["extractedFilters"];
            }
        }

        [ConfigurationProperty("compressors", IsRequired = false)]
        internal ImageCompressorElementCollection ImageCompressors
        {
            get
            {
                return (ImageCompressorElementCollection)base["compressors"];
            }
        }

        [ConfigurationProperty("replacements", IsRequired = false)]
        internal ReplacementElementCollection Replacements
        {
            get
            {
                return (ReplacementElementCollection)base["replacements"];
            }
        }
    }
}