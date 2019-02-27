using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using System.IO;
using Galvarino.Workflow;

namespace Galvarino.Workers
{
    public class CargaInicialCreditosWorker : BackgroundService
    {

        private Timer _timer;
        //private string _connectionString = "server=(LocalDb)\\MSSQLLocalDB;database=galvarino_db;uid=galvarino_db;password=secreto";
        private bool workingThread = false;
        private IWorkflowService _wfservice;


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Timed Background Service is starting.");
            _wfservice = new WorkflowService();
            
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            
            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            
            Console.WriteLine("Esta Trabajando?.");

            if(!workingThread)
            {
                Console.WriteLine("No, empezamos con algo nuevo");
                workingThread = true;
                string processDate = "";
                /*Todo:  Revisar Findesemanas*/
                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    processDate = DateTime.Now.AddDays(-3).ToString("ddMMyyyy");
                }
                else if (
                    DateTime.Now.DayOfWeek == DayOfWeek.Thursday ||
                    DateTime.Now.DayOfWeek == DayOfWeek.Wednesday ||
                    DateTime.Now.DayOfWeek == DayOfWeek.Tuesday ||
                    DateTime.Now.DayOfWeek == DayOfWeek.Friday
                )
                {
                    processDate = DateTime.Now.AddDays(-1).ToString("ddMMyyyy"); 
                }
                

                Console.WriteLine(@"La fecha a Cargar: " + processDate);
                
                

                workingThread = false;
            }
            else
            {
                Console.WriteLine("Si. esperamos que termine");
            }

        }
        

        public override Task StopAsync(CancellationToken cancellation)
        {
            Console.WriteLine("Timed Background Service is stopping.2");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
