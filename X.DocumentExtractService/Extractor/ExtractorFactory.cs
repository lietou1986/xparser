using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using X.DocumentExtractService.Configuration;

namespace X.DocumentExtractService.Extractor
{
    public static class ExtractorFactory
    {
        private static readonly Dictionary<string, List<Func<DocumentExtractor>>> DocumentExtractorFactoryDictionary;

        static ExtractorFactory()
        {
            DocumentExtractorFactoryDictionary = new Dictionary<string, List<Func<DocumentExtractor>>>(StringComparer.OrdinalIgnoreCase);
            foreach (DocumentExtractorElement documentExtractor in DocumentExtractorSection.Current.DocumentExtractors)
            {
                List<Func<DocumentExtractor>> funcs = (from NameValueConfigurationElement extractor in documentExtractor.Extractors where !extractor.Value.IsNullOrWhiteSpace() select GetExtractorFactory(extractor.Value) into extractorFactory where extractorFactory != null select extractorFactory).ToList();
                foreach (NameValueConfigurationElement documentExtension in documentExtractor.DocumentExtensions)
                {
                    if (documentExtension.Value.IsNullOrWhiteSpace())
                    {
                        continue;
                    }
                    DocumentExtractorFactoryDictionary.Add(documentExtension.Value, funcs);
                }
            }
        }

        private static Func<DocumentExtractor> GetExtractorFactory(string extractorName)
        {
            Type type = Type.GetType(string.Concat("X.DocumentExtractService.Extractor.", extractorName));
            return type == null ? null : Expression.Lambda<Func<DocumentExtractor>>(Expression.Convert(Expression.New(type), typeof(DocumentExtractor))).Compile();
        }

        internal static ICollection<DocumentExtractor> GetExtractors(string extension)
        {
            if (!DocumentExtractorFactoryDictionary.ContainsKey(extension))
            {
                return null;
            }
            return (
                from f in DocumentExtractorFactoryDictionary[extension]
                select f()).ToList();
        }
    }
}