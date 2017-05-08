using System;
using System.Collections.Generic;
using System.Linq;
using X.DocumentExtractService.Configuration;
using X.DocumentExtractService.Contract;

namespace X.DocumentExtractService.ExtractedFilters
{
    internal static class ExtractedFilterFactory
    {
        private static readonly IEnumerable<IExtractedFilter> Filters;

        static ExtractedFilterFactory()
        {
            Filters = new List<IExtractedFilter>();
            if (DocumentExtractorSection.Current.ExtractedFilters == null)
            {
                return;
            }
            Filters = (
                from ExtractedFilterElement filterElement in DocumentExtractorSection.Current.ExtractedFilters
                select CreateInstance(filterElement.Name)).ToList();
        }

        private static IExtractedFilter CreateInstance(string filterName)
        {
            return (IExtractedFilter)Activator.CreateInstance(null, string.Concat("X.DocumentExtractService.ExtractedFilters.", filterName)).Unwrap();
        }

        internal static void Filter(ExtractedResult result)
        {
            if (result == null || Filters == null || !Filters.Any())
            {
                return;
            }
            foreach (IExtractedFilter filter in Filters)
            {
                filter.Filter(result);
            }
        }
    }
}