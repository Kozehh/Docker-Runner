using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WorkerObj;

namespace WorkerApp.Controllers
{
    [ApiController]
    [Route("worker")]
    public class WorkerController : ControllerBase
    {
        // GET /worker/getstatus
        [HttpGet]
        [Route("getstatus")]
        public string GetStatus()
        {
            try
            {
                return Program.Api.CheckContainerStatus().Result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "error";
            }
        }

        // POST /worker/startimage
        [HttpPost]
        [Route("startimage")]
        public async void StartImage([FromBody] Worker infoToStart)
        {
            if (!string.IsNullOrEmpty(infoToStart.IPAddress))
            {
                string hostPort = infoToStart.GetFirstPort();
                string containerPort = infoToStart.GetSecondPort();
                Program.Api = new DockerApi(infoToStart.IPAddress, infoToStart.ExeImages[0], hostPort, containerPort);
                try
                {
                    await Program.Api.InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                
            }
        }

        // POST /worker/stopimage
        [HttpPost]
        [Route("stopimage")]
        public async void StopImage([FromBody] string containerToStop)
        {
            try
            {
                await Program.Api.StopContainer(containerToStop);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }
    }
}
