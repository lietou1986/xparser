using System.ComponentModel;

namespace X.ResumeParseService.Host
{
    [RunInstaller(true)]
    public partial class XParserServiceInstaller : System.Configuration.Install.Installer
    {
        public XParserServiceInstaller()
        {
            InitializeComponent();
        }
    }
}