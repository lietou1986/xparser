using System;
using System.Collections.Generic;
using System.Text;
using X.DocumentExtractService.Configuration;
using X.DocumentExtractService.Contract;

namespace X.DocumentExtractService.ExtractedFilters
{
    internal class ReplaceSymbol : IExtractedFilter
    {
        private static readonly Dictionary<string, string> ReplacementRules;

        static ReplaceSymbol()
        {
            ReplacementRules = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (DocumentExtractorSection.Current.Replacements == null)
            {
                return;
            }
            foreach (ReplacementElement replacement in DocumentExtractorSection.Current.Replacements)
            {
                ReplacementRules.Add(replacement.OriginValue, replacement.ReaplaceValue);
            }
        }

        public void Filter(ExtractedResult result)
        {
            if (string.IsNullOrEmpty(result.Text))
            {
                return;
            }
            StringBuilder stringBuilder = new StringBuilder(result.Text);
            foreach (KeyValuePair<string, string> replacementRule in ReplacementRules)
            {
                stringBuilder.Replace(replacementRule.Key, replacementRule.Value);
            }
            result.Text = stringBuilder.ToString();
        }
    }
}