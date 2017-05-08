using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

namespace X.DocumentExtractService.ExtractedFilters.ContractTypes
{
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [GeneratedCode("MSBuild", "14.0.25123.0")]
    [Serializable]
    [XmlRoot(Namespace = "http://PDFExtractionSevice.x.com", IsNullable = false)]
    [XmlType(AnonymousType = true, Namespace = "http://PDFExtractionSevice.x.com")]
    public class ExtractTextFromFileResponse
    {
        private string extractTextFromFileResultField;

        [XmlElement(IsNullable = true)]
        public string ExtractTextFromFileResult
        {
            get
            {
                return extractTextFromFileResultField;
            }
            set
            {
                extractTextFromFileResultField = value;
            }
        }
    }
}