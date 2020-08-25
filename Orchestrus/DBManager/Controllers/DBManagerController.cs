using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WorkerObj;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DBManager.Controllers
{
    [Route("dbmanager")]
    [ApiController]
    public class DBManagerController : ControllerBase
    {
        private static Worker _currentWorker = new Worker();

        // GET /dbmanager/getwrokers
        [HttpGet]
        [Route("getworkers")]
        public List<Worker> GetWorkers()
        {
            return Program.GetWorkers();
        }

        // GET /dbmanager/showimages
        [HttpGet]
        [Route("showimages")]
        public List<string> ShowImages()
        {
            return Program.WorkerImages;
        }

        // GET /dbmanager/getstatus
        [HttpGet]
        [Route("getstatus")]
        public bool GetStatus()
        {
            //return worker isInUse
            return Program.GetStatus(_currentWorker);
        }

        // POST /dbmanager/setstatus
        [HttpPost]
        [Route("setstatus")]
        public void SetStatus([FromBody] Worker workerToUpdate)
        {
            int status;
            if (workerToUpdate.Status.Equals("running"))
            {
                status = 1;
            }
            else
            {
                status = 0;
            }
            Program.UpdateStatus(workerToUpdate.IPAddress, status);
        }

        // POST /dbmanager/setworker
        [HttpPost]
        [Route("setworker")]
        public void SetWorker([FromBody] Worker worker)
        {
            _currentWorker = worker;
        }

        // POST /dbmanager/addworker
        [HttpPost]
        [Route("addworker")]
        public void CreateWorker([FromBody] Worker worker)
        {
            Program.AddWorker(worker);
        }

        // POST /dbmanager/removeworker
        [HttpPost]
        [Route("removeworker")]
        public void RemoveWorker([FromBody] Worker worker)
        {
            Program.RemoveWorker(worker);
        }

        // POST /dbmanager/getimages
        [HttpPost]
        [Route("getimages")]
        public void GetImages([FromBody] string workerAddr)
        {
            Program.WorkerImages = Program.GetImages(workerAddr);
        }

        // POST /dbmanager/startimages
        [HttpPost]
        [Route("startimages")]
        public void StartImages([FromBody] Worker worker)
        {
            Program.StartImages(worker);
        }

        // POST /dbmanager/stopimages
        [HttpPost]
        [Route("stopimages")]
        public void StopImages([FromBody] Worker worker)
        {
            Program.StopImages(worker);
        }
    }
}
