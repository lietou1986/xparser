namespace X.ResumeParseService.Scanner
{
    public class FileActivity
    {
        public FileActivity()
        {
            IsValid = true;
        }

        public FileActivity(string filePath, bool isChanged = false) : this()
        {
            FilePath = filePath;
            IsChanged = isChanged;
        }

        public string HashCode { get; set; }
        public string FilePath { get; set; }
        public bool IsChanged { get; set; }
        public bool IsValid { get; set; }
        public string Md5 { get; set; }
        public string Message { get; set; }
    }
}