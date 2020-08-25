using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WorkerObj;

namespace DBManager
{
    public class Program
    {
        private static Database orcherstrusDB;
        public static List<string> WorkerImages = new List<string>();

        public static void Main(string[] args)
        {
        	Thread.Sleep(5000);
            orcherstrusDB = new Database("database", "root", "password", "mydatabase", "3306");
            orcherstrusDB.Connect();
            orcherstrusDB.InitialiseTables();
            
            CreateHostBuilder(args).Build().Run();
        }

        public static void AddWorker(Worker worker)
        {
            orcherstrusDB.AddWorker(worker);
        }

        public static void RemoveWorker(Worker worker)
        {
            orcherstrusDB.RemoveWorker(worker);
        }

        public static List<Worker> GetWorkers()
        {
            List<Worker> workersList = orcherstrusDB.GetWorkers();
            foreach (Worker worker in workersList)
            {
                worker.ExeImages = orcherstrusDB.GetImages(worker.IPAddress);
            }
            return orcherstrusDB.GetWorkers();
        }

        public static bool GetStatus(Worker worker)
        {
            return orcherstrusDB.GetStatus(worker);
        }

        
        public static void UpdateStatus(string workerAdress, int isActive)
        {
            orcherstrusDB.UpdateStatus(workerAdress, isActive);
        }

        public static void StartImages(Worker worker)
        {
            foreach(string image in worker.ExeImages)
            {
                orcherstrusDB.AddImage(worker.IPAddress, image);
            }
            if(worker.ExeImages.Count() != 0)
            {
                orcherstrusDB.UpdateStatus(worker.IPAddress, 1);
            }
        }

        public static void StopImages(Worker worker)
        {
            foreach (string image in worker.ExeImages)
            {
                orcherstrusDB.RemoveImage(worker.IPAddress, image);
            }
            if(orcherstrusDB.GetImages(worker.IPAddress).Count() == 0)
            {
                orcherstrusDB.UpdateStatus(worker.IPAddress, 0);
            }
        }

        public static List<string> GetImages(string workerAdress)
        {
            return orcherstrusDB.GetImages(workerAdress);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
