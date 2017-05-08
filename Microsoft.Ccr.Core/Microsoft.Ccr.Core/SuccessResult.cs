namespace Microsoft.Ccr.Core
{
    public class SuccessResult
    {
        private static SuccessResult _instance = new SuccessResult();

        private readonly int m_Status;

        private readonly string m_StrStatus;

        public static SuccessResult Instance
        {
            get
            {
                return SuccessResult._instance;
            }
        }

        public string StatusMessage
        {
            get
            {
                return m_StrStatus;
            }
        }

        public int Status
        {
            get
            {
                return m_Status;
            }
        }

        public SuccessResult()
        {
        }

        public SuccessResult(int status)
        {
            m_Status = status;
        }

        public SuccessResult(string status)
        {
            m_StrStatus = status;
        }
    }
}