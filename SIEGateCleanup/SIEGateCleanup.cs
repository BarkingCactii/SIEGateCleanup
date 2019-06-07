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
using System.IO;

namespace SIEGateCleanup
{
    public partial class SIEGateCleanup : ServiceBase
    {
        private static readonly Logger.Logger _log = Logger.Log.GetInstance("SIEGateCleanup");

        private static Timer aTimer;
        private string _path = null;

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
            StartProcess(args);
        }

        protected override void OnStop()
        {
            StopProcess();
        }

        public void StartProcess(string [] args)
        {
            _path = args[0];
            aTimer = new Timer(2 * 1000);
            aTimer.Elapsed += new ElapsedEventHandler(ExecuteEveryDayMethod);
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void ExecuteEveryDayMethod(object source, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine(String.Format("{0:yyyy-MM-dd HH:mm:ss} : Cleaning...", e.SignalTime));
                // after initial execution, set timer to its repeating state
                aTimer.Interval = 60 * 1000;

                PurgeFiles(_path);
            }
            catch
            {
            }
        }

        private void PurgeFiles(string dirPath)
        {
            try
            {
                string[] folders = Directory.GetDirectories(dirPath);

                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        //    File.Delete(file);
                        _log.Info(file);
                    //    Console.WriteLine("Deleting " + file);
                    }
                }
            }
            catch ( Exception ex)
            {

            }
        }

        public void StopProcess()
        {

        }

    }
}
