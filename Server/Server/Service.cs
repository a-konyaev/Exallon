using System.ServiceProcess;

namespace Exallon
{
    public partial class Service : ServiceBase
    {
        private Application _application;

        public Service()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (_application != null)
                return;

            _application = new Application();
        }

        protected override void OnStop()
        {
            if (_application == null)
                return;

            _application.Dispose();
        }
    }
}
