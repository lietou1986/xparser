using Dorado.Core;
using Dorado.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using X.DocumentExtractService.Compressors;
using X.DocumentExtractService.Configuration;
using X.DocumentExtractService.Contract;
using X.DocumentExtractService.Contract.Models;
using X.DocumentExtractService.ExtractedFilters;

namespace X.DocumentExtractService.Extractor
{
    internal abstract class DocumentExtractor
    {
        private static readonly Dictionary<PictureCategory, List<ImageSizeElement>> CacheImageSizePictureTypes;

        private static readonly string SupportImageTypes;

        static DocumentExtractor()
        {
            CacheImageSizePictureTypes = new Dictionary<PictureCategory, List<ImageSizeElement>>();
            foreach (ImageSizeElement imageCompressor in DocumentExtractorSection.Current.ImageCompressors)
            {
                if (CacheImageSizePictureTypes.ContainsKey(imageCompressor.PictureCategory))
                {
                    CacheImageSizePictureTypes[imageCompressor.PictureCategory].Add(imageCompressor);
                }
                else
                {
                    CacheImageSizePictureTypes.Add(imageCompressor.PictureCategory, new List<ImageSizeElement>(new[] { imageCompressor }));
                }
            }
            SupportImageTypes = ConfigurationManager.AppSettings["supportedImageType"];
        }

        protected abstract bool CanBeExtracted(string extensionName, byte[] data);

        protected virtual bool CheckImageType(string imageType)
        {
            if (imageType.IsNullOrWhiteSpace())
            {
                return false;
            }
            return SupportImageTypes.Contains(imageType.Replace(".", "").ToLower());
        }

        private Picture[] CompressImages(Picture[] images)
        {
            if (DocumentExtractorSection.Current.ImageCompressors.Count > 0)
            {
                List<Picture> pictures = new List<Picture>();
                Picture[] pictureArray = images;
                foreach (Picture picture in pictureArray)
                {
                    if (CacheImageSizePictureTypes.ContainsKey(picture.PictureCategory))
                    {
                        if (!pictures.Contains(picture))
                        {
                            pictures.Add(picture);
                        }
                        foreach (ImageSizeElement item in CacheImageSizePictureTypes[picture.PictureCategory])
                        {
                            byte[] compressedImageBytes = (new HighQualityPictureCompressor(picture.Data)).GetCompressedImageBytes(item.Width, item.Height);
                            Picture picture1 = new Picture()
                            {
                                Data = compressedImageBytes,
                                Width = item.Width,
                                Height = item.Height,
                                PictureCategory = item.PictureCategory,
                                Tag = item.Name,
                                Extension = picture.Extension
                            };
                            picture.RelatedPictures.Add(picture1);
                        }
                    }
                }
                images = pictures.ToArray();
            }
            return images;
        }

        public ExtractedResult Extract(string extensionName, byte[] data, ExtractOption extractOptions)
        {
            if (!CanBeExtracted(extensionName, data))
            {
                return null;
            }
            ExtractedResult extractedResult1 = new ExtractedResult();
            if ((extractOptions & ExtractOption.Text) == ExtractOption.Text)
            {
                try
                {
                    extractedResult1.Text = ExtractText(extensionName, data);
                    if ((extractOptions & ExtractOption.Image) == ExtractOption.Image)
                    {
                        try
                        {
                            extractedResult1.Images = ExtractAndCompressImages(extensionName, data);
                        }
                        catch (Exception exception)
                        {
                            LoggerWrapper.Logger.Error("抽取图片", exception);
                        }
                    }
                    ExtractedFilterFactory.Filter(extractedResult1);
                    return extractedResult1;
                }
                catch (Exception exception1)
                {
                    LoggerWrapper.Logger.Error("抽取文本", exception1);
                }
                return null;
            }
            if ((extractOptions & ExtractOption.Image) == ExtractOption.Image)
            {
                try
                {
                    extractedResult1.Images = ExtractAndCompressImages(extensionName, data);
                }
                catch (Exception exception)
                {
                    LoggerWrapper.Logger.Error("抽取图片", exception);
                }
            }
            ExtractedFilterFactory.Filter(extractedResult1);
            return extractedResult1;
        }

        private Picture[] ExtractAndCompressImages(string extensionName, byte[] data)
        {
            Picture[] array = ExtractImages(extensionName, data);
            if (array != null && array.Length != 0)
            {
                array = array.Where(pic =>
                {
                    if (!CheckImageType(pic.Extension))
                    {
                        return false;
                    }
                    pic.PictureCategory = PictureRecognizers.PictureRecognizers.GetPictureType(pic);
                    return true;
                }).ToArray();
                array = CompressImages(array);
            }
            return array;
        }

        protected virtual Picture[] ExtractImages(string extensionName, byte[] data)
        {
            return null;
        }

        protected abstract string ExtractText(string extensionName, byte[] data);

        protected Size GetImageSize(Stream stream)
        {
            Image image = Image.FromStream(stream);
            return new Size(image.Width, image.Height);
        }
    }
}