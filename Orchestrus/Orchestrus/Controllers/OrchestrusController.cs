using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WorkerObj;

namespace Orchestrus.Controllers
{ 
    [Route("api")]
    [ApiController]
    public class OrchestrusController : ControllerBase
    {
        public HttpClient _client = new HttpClient();
        public static List<string> workerImages = new List<string>();
        public static string dbManagerAddr = Environment.GetEnvironmentVariable("DB_MANAGER_ADDR");

        // Get /api/showimages
        [HttpGet]
        [Route("showimages")]
        public List<string> ShowImages()
        {
            return workerImages;
        }

        // Get /api/showworker
        [HttpGet]
        [Route("showworker")]
        public List<Worker> GetActiveWorkers()
        {
            try
            {
                var getWrkRes = _client.GetAsync(dbManagerAddr + "/dbmanager/getworkers").Result;
                return JsonConvert.DeserializeObject<List<Worker>>(getWrkRes.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        // Post /api/getimages
        [HttpPost]
        [Route("getimages")]
        public void ShowImages([FromBody] string workerAddr)
        {
            var serialize = JsonConvert.SerializeObject(workerAddr);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            try
            {
                var postResult = _client.PostAsync(dbManagerAddr + "/dbmanager/getimages", toSend).Result;
                var getResult = _client.GetAsync(dbManagerAddr + "/dbmanager/showimages").Result;
                workerImages = JsonConvert.DeserializeObject<List<string>>(getResult.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Post /api/startimage
        [HttpPost]
        [Route("startimage")]
        public void StartImage([FromBody] Worker worker)
        {
            _client.Timeout = TimeSpan.FromSeconds(10);
            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");

            try
            {
                var addWorkerRes =  _client.PostAsync(dbManagerAddr + "/dbmanager/addworker", toSend).Result;
                // To be sure the above query is finished
                Thread.Sleep(3000);
                var startImgRes =  _client.PostAsync(dbManagerAddr + "/dbmanager/startimages", toSend).Result;
                var startContainerRes = _client.PostAsync($"http://{worker.IPAddress}:9002/worker/startimage", toSend).Result;
                Program.workerAddresses.Add(worker.IPAddress);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // Post /api/stopimage
        [HttpPost]
        [Route("stopimage")]
        public void StopWorker([FromBody] Worker worker)
        {
            var serialize = JsonConvert.SerializeObject(worker.ExeImages[0]);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            try
            {
                var stopImgRes =  _client.PostAsync($"http://{worker.IPAddress}:9002/worker/stopimage", toSend).Result;
                var status = _client.GetAsync($"http://{worker.IPAddress}:9002/worker/getstatus").Result; worker.Status = status.ToString();
                serialize = JsonConvert.SerializeObject(worker);
                toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
                var setStatusRes = _client.PostAsync(dbManagerAddr + "/dbmanager/stopimages", toSend).Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected virtual string GetServiceAddr()
        {
            return Environment.GetEnvironmentVariable("DB_MANAGER_ADDR");
        }
    }

}
