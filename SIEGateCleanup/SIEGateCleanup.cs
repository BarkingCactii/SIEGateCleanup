using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SIEGateCleanup
{
    public partial class SIEGateCleanup : ServiceBase
    {
        private static Timer aTimer;

        public void RunAsConsole(string[] args)
        {
            OnStart(args);
            Console.WriteLine("Running service as a Console. Press Enter to stop program");
            Console.ReadLine();
            OnStop();
        }

        public SIEGateCleanup()
        {
            InitializeComponent();

        }

        protected override void OnStart(string[] args)
        {
            StartProcess();
        }

        protected override void OnStop()
        {
            StopProcess();
        }

        public void StartProcess()
        {
            aTimer = new Timer(10 * 1000);
            aTimer.Elapsed += new ElapsedEventHandler(ExecuteEveryDayMethod);
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void ExecuteEveryDayMethod(object source, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine(String.Format("{0:yyyy-MM-dd HH:mm:ss} : Cleaning...", e.SignalTime));// DateTime.Now));
            }
            catch
            {
            }
        }

        public void StopProcess()
        {

        }

    }
}
