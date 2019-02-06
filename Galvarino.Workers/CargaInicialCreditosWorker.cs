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

namespace Galvarino.Workers
{
    public class CargaInicialCreditosWorker : BackgroundService
    {

        private Timer _timer;
        private string _connectionString = "server=(LocalDb)\\MSSQLLocalDB;database=galvarino_db;uid=galvarino_db;password=secreto";
        private bool workingThread = false;


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Timed Background Service is starting.");
            
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

                Thread.Sleep(TimeSpan.FromSeconds(10));

                Console.WriteLine(@"despues de 10 segundos, se cierra la tarea y abrimos el semasforo");

                

                workingThread = false;
            }
            else
            {
                Console.WriteLine("Si. esperamos que termine");
            }

        }

        private void DoProcess(string processDate)
        {
            string path = @"E:\Carga" + processDate + ".txt"; ;

            if (File.Exists(path))
            {
                int lap = 0;
                
            }


            /*using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    
                }*/
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
