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
using System.Collections;
using System.Collections.Specialized;

namespace SIEGateCleanup
{
    public partial class SIEGateCleanup : ServiceBase
    {
        private static readonly Logger.Logger _log = Logger.Log.GetInstance("SIEGateCleanup");
        private static DateTime upTime = DateTime.Now;
        OrderedDictionary _stats = new OrderedDictionary();

        private static Timer aTimer;
        private string _path = null;

        class CleanupStats
        {
            public long totalBytes;
            public int totalFiles;
        }

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
                _log.Info("Thread started");
                // after initial execution, set timer to its repeating state
                aTimer.Interval = 60 * 1000;

                CleanupStats stat = PurgeFiles(_path);
                _stats.Add(DateTime.Now, stat);

                PrintStats();
                //_log.Info(String.Format("Summary: Files deleted {0}, Total bytes {1:n0}mb", stats.totalFiles, stats.totalBytes / (1024 * 1024)));
            }
            catch
            {
            }
        }

        private void PrintStats()
        {
            _log.Info(String.Format("Summary as of {0}", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            _log.Info(String.Format("{0,-25}{1,11}{2,15}", "Time", "Total Files","Total Mb"));
            foreach ( DateTime item in _stats.Keys)
            {
                CleanupStats currentStat = (CleanupStats)_stats[item];
                _log.Info(String.Format("{0,-25}{1,11}{2,15:n0} Mb", item.ToString("dd/MM/yyyy HH:mm:ss"), currentStat.totalFiles, currentStat.totalBytes / (1024 * 1024)));
            }
            _log.Info("END");
        }

        private CleanupStats PurgeFiles(string dirPath)
        {
            CleanupStats stats = new CleanupStats() { totalBytes = 0, totalFiles = 0 };

            try
            {
                string[] folders = Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories);

                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);//.AllDirectories);
                    if ( files.Length > 0)
                        _log.Info(String.Format("Purging folder {0} -> {1} files", folder, files.Length));

                    foreach (string file in files)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file);
                            stats.totalBytes += fi.Length;
                            stats.totalFiles++;
                            //    File.Delete(file);
                //            _log.Info(file);
                            //    Console.WriteLine("Deleting " + file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Inner loop: " + ex.Message);
                        }
                    }
                }
            }
            catch ( Exception ex)
            {
                Console.WriteLine("Outer loop: " + ex.Message);
            }
            return stats;
        }

        public void StopProcess()
        {

        }

    }
}
