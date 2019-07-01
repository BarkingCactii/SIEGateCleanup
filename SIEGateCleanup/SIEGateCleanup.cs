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
using System.Configuration;

namespace SIEGateCleanup
{
    public partial class SIEGateCleanup : ServiceBase
    {
        private static readonly Logger.Logger _log = Logger.Log.GetInstance("log");
        private static DateTime upTime = DateTime.Now;
        OrderedDictionary _stats = new OrderedDictionary();
        OrderedDictionary _dailyTotals = new OrderedDictionary();

        private int _minutes = 60 * 12; // twice a day default
        private int _daysToKeep = 7; // once a week
        private bool _simulate = false;

        private static Timer aTimer;
        private string [] _path = null;

        class CleanupStats
        {
            public long totalBytes;
            public int totalFiles;
            public string folder;
        }

        public void RunAsConsole(string[] args)
        {
            if (args.Length > 1) {
                if (args[1].Contains("simulate"))
                {
                    _simulate = true;
                    _log.Info("Running in simulated mode. No files will be deleted");
                }
            }

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

            if ( int.TryParse(ConfigurationManager.AppSettings["Minutes"], out _minutes))
                _log.Info(String.Format("Applying thread restart every {0} minutes", _minutes ));

            if (int.TryParse(ConfigurationManager.AppSettings["DaysToKeep"], out _daysToKeep))
                _log.Info(String.Format("Applying Days to keep archives to {0} days", _daysToKeep));

            string[] seperator = { ";" };
            string[] path = null;

            if (args.Length > 0)
                path = args[0].Split(seperator, StringSplitOptions.None);
            else
                path = ConfigurationManager.AppSettings["Path"].Split(seperator, StringSplitOptions.None);

            if (path == null || path.Length == 0)
            {
                _log.Error("No path has been set. Cannot continue");
                OnStop();
                return;
            }

            StartProcess(path);
        }

        protected override void OnStop()
        {
            StopProcess();
        }

        public void StartProcess(string [] args)
        {
            _log.Info("Applying purging to folders " + string.Join(";", args));
            _path = args;
            aTimer = new Timer(2 * 1000);
            aTimer.Elapsed += new ElapsedEventHandler(ExecuteEveryDayMethod);
            aTimer.Enabled = true;
        }

        // Specify what you want to happen when the Elapsed event is raised.
        private void ExecuteEveryDayMethod(object source, ElapsedEventArgs e)
        {
            try
            {
                _log.Info("Process started");

                // after initial execution, set timer to its repeating state
                aTimer.Interval = _minutes * 60 * 1000;

                foreach (string folder in _path)
                {
                    PurgeFiles(folder);
                }

                PrintStats();
                PrintDailyTotal();

                _stats.Clear();
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message);
            }
            finally
            {
            }
        }

        private void PrintStats()
        {
            char topleft = '┌';
            char hline = '─';
            char topright = '┐';
            char vline = '│';
            char bottomleft = '└';
            char bottomright = '┘';

            var builder = new StringBuilder();
            builder.Append(topleft);
            for (int i = 0; i < 104; i++)
                builder.Append(hline);
            builder.Append(topright);
            _log.Info(builder.ToString());

            _log.Info(String.Format("{0}{1,-14}{2,-90}{0}", vline, "Summary as of ", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")));
            _log.Info(String.Format("{0}{1,-22}{2,-60}{3,11}{4,11}{0}", vline, "Time", "Folder", "Total Files", "Total Mb"));
            foreach ( DateTime item in _stats.Keys)
            {
                CleanupStats currentStat = (CleanupStats)_stats[item];
                _log.Info(String.Format("{0}{1,-22}{2,-60}{3,11}{4,11:n0}{0}", vline, item.ToString("dd/MM/yyyy HH:mm:ss"), currentStat.folder, currentStat.totalFiles, currentStat.totalBytes / (1024 * 1024)));
            }

            builder.Clear();
            builder.Append(bottomleft);
            for (int i = 0; i < 104; i++)
                builder.Append(hline);
            builder.Append(bottomright);
            _log.Info(builder.ToString());
        }

        private void PrintDailyTotal()
        {
            char topleft = '┌';
            char hline = '─';
            char topright = '┐';
            char vline = '│';
            char bottomleft = '└';
            char bottomright = '┘';

            var builder = new StringBuilder();
            builder.Append(topleft);
            for (int i = 0; i < 104; i++)
                builder.Append(hline);
            builder.Append(topright);
            _log.Info(builder.ToString());

            _log.Info(String.Format("{0}{1,-104}{0}", vline, "Daily Total Summary"));
            _log.Info(String.Format("{0}{1,-33}{2,-71}{0}", vline, "Date", "Total Mb"));
            foreach (DateTime item in _dailyTotals.Keys)
            {
                long bytes = (long)_dailyTotals[item];
                _log.Info(String.Format("{0}{1,-33}{2,-71:n0}{0}", vline, item.ToString("dd/MM/yyyy"), bytes / (1024 * 1024)));
            }

            builder.Clear();
            builder.Append(bottomleft);
            for (int i = 0; i < 104; i++)
                builder.Append(hline);
            builder.Append(bottomright);
            _log.Info(builder.ToString());
        }

        private void PurgeFiles(string dirPath)
        {
            try
            {
                DateTime today = DateTime.Now.Date;
                TimeSpan keepBuffer = new TimeSpan(_daysToKeep, 0, 0, 0);

                if (!_dailyTotals.Contains(today))
                    _dailyTotals[today] = (long)0;

                string[] folders = Directory.GetDirectories(dirPath, "*", SearchOption.AllDirectories);

                foreach (string folder in folders)
                {
                    string[] files = Directory.GetFiles(folder, "*", SearchOption.TopDirectoryOnly);

                    if (files.Length == 0)
                        // don't process empty folders
                        continue;

                    _log.Info(String.Format("Purging folder {0} -> {1} files", folder, files.Length));

                    CleanupStats stats = new CleanupStats() { totalBytes = 0, totalFiles = 0 };

                    foreach (string file in files)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file);
                            stats.totalBytes += fi.Length;
                            stats.totalFiles++;
                            stats.folder = folder;

                            try
                            {
                                if (fi.LastWriteTime > today - keepBuffer)
                                {
                                    // recently created archive, so don't delete
                                    _log.Debug("Not deleting recent file " + file);
                                    continue;
                                }

                                if (_simulate == true)
                                    _log.Debug("Would have deleted " + file);
                                else
                                {
                                    File.Delete(file);
                                    _dailyTotals[today] = (object)((long)_dailyTotals[today] + stats.totalBytes);
                                }
                            }
                            catch (Exception ex)
                            {
                                _log.Error("File.Delete try PurgeFiles: " + ex.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.Error("Inner try PurgeFiles: " + ex.Message);
                        }
                    }

                    _stats.Add(DateTime.Now, stats);
                }
            }
            catch ( Exception ex)
            {
                _log.Error("Outer try PurgeFiles: " + ex.Message);
            }
        }

        public void StopProcess()
        {
            aTimer.Enabled = false;
            aTimer.Stop();
        }
    }
}
