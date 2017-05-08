namespace Microsoft.Ccr.Core
{
    public class Shutdown
    {
        private SuccessFailurePort _resultPort = new SuccessFailurePort();

        public SuccessFailurePort ResultPort
        {
            get
            {
                return _resultPort;
            }
            set
            {
                _resultPort = value;
            }
        }
    }
}