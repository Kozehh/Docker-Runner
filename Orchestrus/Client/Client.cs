using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkerObj;

namespace Client
{
    class Client
    {
        public const string HelpCmd = "--help";
        public const string ShowRunningWorkerCmd = "worker";
        public const string ShowWorkerImagesCmd = "images";
        public const string StartWorerCmd = "start";
        public const string StopImageCmd = "stop";
        public static string serverAddr;
        private static HttpClient _client;

        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            var terminated = false;
            serverAddr = Environment.GetEnvironmentVariable("SERVER_ADDR");

            if (string.IsNullOrEmpty(serverAddr))
            {
                Console.WriteLine("Variable d'environnnement invalide");
                return; // quitter le programme
            }

            _client = new HttpClient();
            Console.WriteLine("Veuillez gentillement utiliser '--help' pour voir les commandes disponible");
            while (!terminated)
            {
                Console.WriteLine("Qu'elle commande puis-je exécuter pour vous aujourd'hui ?");
                var command = Console.ReadLine();

                if (command.Equals(HelpCmd))
                {
                    ShowHelp();
                }
                else if (command.Equals(ShowWorkerImagesCmd))
                {
                    Console.WriteLine("Veuillez indiquer l'adresse IP du worker que vous voulez voir les images en exécution");
                    ShowImages(Console.ReadLine());
                }
                else if (command.Equals(ShowRunningWorkerCmd))
                {
                    ShowWorker();
                }
                else if (command.Equals(StartWorerCmd))
                {
                    Console.WriteLine("Veuillez indiquer l'adresse IP pour l'hote a utiliser");
                    var workerAddr = Console.ReadLine();
                    Console.WriteLine("Veuillez indiquer le nom de l'image que vous voulez exécuter");
                    var image = Console.ReadLine();
                    Console.WriteLine("Veuillez indiquer la cartographie de port a utiliser pour le travailleur");
                    var ports = Console.ReadLine();
                    StartImage(workerAddr, image, ports);
                }
                else if (command.Equals(StopImageCmd))
                {
                    Console.WriteLine("Veuillez indiquer l'image que vous voulez arrêter");
                    var image = Console.ReadLine();
                    Console.WriteLine("Veuillez indiquer l'adresse IP du worker contenant cette image lancée");
                    StopImage(image, Console.ReadLine());
                }
                else if (command.Equals("t"))
                {
                    terminated = true;
                }
            }
        }

        private static void ShowWorker()
        {
            try
            {
                var returnValue = _client.GetAsync(serverAddr + "/api/showworker").Result;
                var content = returnValue.Content.ReadAsStringAsync().Result;
                var workers = JsonConvert.DeserializeObject<List<Worker>>(content);
                foreach (var worker in workers)
                {
                    Console.WriteLine(worker.IPAddress);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void StartImage(string workerAddr, string image, string ports)
        {
            Worker worker = new Worker()
            {
                IPAddress = workerAddr,
                Ports = ports,
                ExeImages = new List<string>{image}
            };

            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            try
            {
                var startImgRes = _client.PostAsync(serverAddr + "/api/startimage", toSend).Result;
                Console.WriteLine($"Sucessfully started image {image} with ports {ports} on worker {workerAddr}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ShowImages(string workerAddr)
        {
            var serialize = JsonConvert.SerializeObject(workerAddr);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            try
            {
                var res = _client.PostAsync(serverAddr + "/api/getimages", toSend).Result;
                
                var result = _client.GetAsync(serverAddr + "/api/showimages").Result;
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception("Error: REST call wasn't successful. Please try again");
                }
                var images = JsonConvert.DeserializeObject<List<string>>(result.Content.ReadAsStringAsync().Result);
                Console.WriteLine("Voici la liste de tous les images qui sont en exécution sur le worker " +
                                  workerAddr);
                foreach (string image in images)
                {
                    Console.WriteLine("  -  " + image);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void StopImage(string image, string workerAddr)
        {
            Worker worker = new Worker()
            {
                IPAddress = workerAddr,
                ExeImages = new List<string> {image}
            };

            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            try
            {
                var stopImgRes = _client.PostAsync(serverAddr + "/api/stopimage", toSend).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Commandes disponible avec Orchestrus :\n" +
                "image : Pour afficher les images en exécution dans un worker spécifié avec son adresse IP\n" +
                "worker : Pour afficher les workers connectés\n" +
                "start : Partir une image dans un travailleur avec la cartographie de port\n" +
                "stop : Arrêter le travail d'un travailleur spécifié avec son adresse IP\n" +
                "t : Terminate the application\n");
        }
    }
}