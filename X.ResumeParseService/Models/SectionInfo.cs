using Newtonsoft.Json;

namespace X.ResumeParseService.Models
{
    public class SectionInfo
    {
        public int Start { get; set; }
        public int End { get; set; }

        public SectionInfo(int start, int end)
        {
            this.Start = start;
            this.End = end;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}