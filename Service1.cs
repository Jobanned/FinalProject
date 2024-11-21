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

namespace finalprojbackup
{
    partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer; // Specify System.Timers.Timer
        private SupplyChainSystem supplySystem;

        public Service1()
        {
            InitializeComponent();
            supplySystem = new SupplyChainSystem();
        }

        protected override void OnStart(string[] args)
        {
            Log("Service started.");

            // Set up a timer to perform tasks every 10 minutes
            timer = new System.Timers.Timer(600000); // 10 minutes in milliseconds
            timer.Elapsed += PerformSupplyChainTasks;
            timer.Start();
        }

        protected override void OnStop()
        {
            Log("Service stopped.");
            timer?.Stop();
        }

        private void PerformSupplyChainTasks(object sender, ElapsedEventArgs e)
        {
            try
            {
                // Example: Add a sample supplier to the file
                supplySystem.AddSupplier();

                // Example: Log all suppliers
                using (var sw = new StreamWriter("C:\\ServiceLogs\\suppliers_log.txt", true))
                {
                    var suppliers = File.ReadAllLines("suppliers.txt");
                    foreach (var supplier in suppliers)
                    {
                        sw.WriteLine($"{DateTime.Now}: {supplier}");
                    }
                }

                Log("Supply chain task executed.");
            }
            catch (Exception ex)
            {
                Log($"Error: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            using (StreamWriter sw = new StreamWriter("C:\\ServiceLogs\\service_log.txt", true))
            {
                sw.WriteLine($"{DateTime.Now}: {message}");
            }
        }
    }
}
