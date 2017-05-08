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
    public class ExtractTextFromFileData
    {
        private string fileNameField;

        private byte[] fileDataField;

        [XmlElement(DataType = "base64Binary", IsNullable = true)]
        public byte[] fileData
        {
            get
            {
                return fileDataField;
            }
            set
            {
                fileDataField = value;
            }
        }

        [XmlElement(IsNullable = true)]
        public string fileName
        {
            get
            {
                return fileNameField;
            }
            set
            {
                fileNameField = value;
            }
        }
    }
}