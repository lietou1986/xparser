using System;
using System.Configuration;
using X.DocumentExtractService.Contract.Models;

namespace X.DocumentExtractService.Configuration
{
    public class ImageSizeElement : ConfigurationElement
    {
        [ConfigurationProperty("height")]
        internal int Height
        {
            get
            {
                return (int)base["height"];
            }
        }

        [ConfigurationProperty("name")]
        internal string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        public PictureCategory PictureCategory
        {
            get
            {
                return (PictureCategory)Enum.Parse(typeof(PictureCategory), Type, true);
            }
        }

        [ConfigurationProperty("type")]
        internal string Type
        {
            get
            {
                return (string)base["type"];
            }
        }

        [ConfigurationProperty("width")]
        internal int Width
        {
            get
            {
                return (int)base["width"];
            }
        }
    }
}