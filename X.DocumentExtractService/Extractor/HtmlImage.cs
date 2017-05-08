using System.Collections.Generic;

namespace X.DocumentExtractService.Extractor
{
    public class HtmlImage
    {
        private static readonly double _DefaultValue;

        private Dictionary<string, string> m_Properties = new Dictionary<string, string>();

        private double? m_Width;

        private double? m_Height;

        public double Height
        {
            get
            {
                if (!m_Height.HasValue)
                {
                    double num;
                    if (!m_Properties.ContainsKey("height") || !double.TryParse(m_Properties["height"], out num))
                    {
                        return _DefaultValue;
                    }
                    m_Height = new double?(num);
                }
                return m_Height.Value;
            }
            set
            {
                m_Height = new double?(value);
            }
        }

        public double RateOfSize
        {
            get
            {
                if (Width == 0)
                {
                    return 0;
                }
                return Height / Width;
            }
        }

        public string Src
        {
            get
            {
                if (!m_Properties.ContainsKey("src"))
                {
                    return null;
                }
                return m_Properties["src"];
            }
        }

        public double Width
        {
            get
            {
                double num;
                if (!m_Width.HasValue)
                {
                    if (!m_Properties.ContainsKey("width") || !double.TryParse(m_Properties["width"], out num))
                    {
                        return _DefaultValue;
                    }
                    m_Width = new double?(num);
                }
                return m_Width.Value;
            }
            set
            {
                m_Width = new double?(value);
            }
        }

        static HtmlImage()
        {
            _DefaultValue = -1;
        }

        public void AddProperty(string name, string value)
        {
            m_Properties[name] = value;
        }
    }
}