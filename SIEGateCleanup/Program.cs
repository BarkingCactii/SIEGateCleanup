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
                /*
                 * To install as a service run c:\windows\Microsoft.NET\Framework\v4.0.30319\installutil SIEGateCleanup.exe
                 * To uninstall run c:\windows\Microsoft.NET\Framework\v4.0.30319\installutil /u SIEGateCleanup.exe
                 */
                service.RunAsConsole(args);
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { service };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
