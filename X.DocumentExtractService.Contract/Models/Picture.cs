using System.Collections.Generic;

namespace X.DocumentExtractService.Contract.Models
{
    public class Picture
    {
        private ICollection<Picture> _mRelatedPictures = new List<Picture>();

        public byte[] Data
        {
            get;
            set;
        }

        /// <summary>
        /// 图片后缀名
        /// </summary>
        public string Extension
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public PictureCategory PictureCategory
        {
            get;
            set;
        }

        /// <summary>
        /// 用于存储一些有关系的图像
        /// </summary>
        public ICollection<Picture> RelatedPictures
        {
            get
            {
                return _mRelatedPictures;
            }
            private set
            {
                _mRelatedPictures = value;
            }
        }

        public string Tag
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }
    }
}