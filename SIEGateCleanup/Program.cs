using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SIEGateCleanup
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            SIEGateCleanup service = new SIEGateCleanup();
            if (Environment.UserInteractive)
            {

                string path =  ConfigurationManager.AppSettings["Path"];
//                    System.Configuration.Configuration.AppSettings.CurrentConfiguration.
 //                  string value = System.Configuration.ConfigurationManager.AppSettings["Path"];

                service.RunAsConsole(args);
            }
            else
            {
                string value = System.Configuration.ConfigurationManager.AppSettings["Path"];

                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { service };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
