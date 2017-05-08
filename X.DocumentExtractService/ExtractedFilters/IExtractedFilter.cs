using X.DocumentExtractService.Contract;

namespace X.DocumentExtractService.ExtractedFilters
{
    internal interface IExtractedFilter
    {
        void Filter(ExtractedResult result);
    }
}