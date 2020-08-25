using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Orchestrus.Controllers;
using WorkerObj;

namespace Orchestrus
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly HttpClient _client = new HttpClient();

        // On fait des call periodiquement aux workers pour update les status des images lancees
        private void GetUpdates(object state)
        {
            try
            {
                if (Program.workerAddresses != null)
                {
                    foreach (string workerAddr in Program.workerAddresses)
                    {
                        var result = _client.GetAsync($"http://{workerAddr}:9002" + "/worker/getstatus").Result;
                        var status = JsonConvert.DeserializeObject<string>(result.Content.ReadAsStringAsync().Result);

                        Worker worker = new Worker()
                        {
                            IPAddress = workerAddr,
                            Status = status.ToString()
                        };
                        var serialize = JsonConvert.SerializeObject(worker);
                        var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
                        _client.PostAsync(OrchestrusController.dbManagerAddr + "/dbmanager/setstatus", toSend);
                    }
                }
                
            }catch(Exception ex)
            {
                // Exception occured
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _timer = new Timer(GetUpdates, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
