using Newtonsoft.Json;

namespace X.ResumeParseService.Contract.Models
{
    public class CertificateData
    {
        public string CertificateTitle { get; set; }
        public string AcquireTime { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}