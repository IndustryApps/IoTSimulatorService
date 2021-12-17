using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace IOTSimulatorService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new SimulatorService()
            };
            ServiceBase.Run(ServicesToRun);

            ////uncomment below code and comment above codes for debugging
            //SimulatorService service = new SimulatorService();
            //service.Run();
            //System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }


    }
}
