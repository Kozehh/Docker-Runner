using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WorkerObj;

namespace Test
{
    [TestClass]
    class WorkerAppTest
    {
        public static string hostAddr = OrchestrusTest.GetLocalIPAddress();

        [TestMethod]
        public void StartImageInWorkerTest()
        {
            // Act
            HttpClient client = new HttpClient();
            Worker worker = new Worker()
            {
                IPAddress = hostAddr,
                Ports = "9000:80",
                ExeImages = new List<string> { "httpd" }
            };

            // Action
            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRes = client.PostAsync( hostAddr + ":9002/worker/startimage", toSend).Result;

            // Assert
            Assert.IsTrue(startImgRes.IsSuccessStatusCode);
        }

        [TestMethod]
        public void StopImageInWorkerTest()
        {
            // Act
            HttpClient client = new HttpClient();

            // Action
            var serialize = JsonConvert.SerializeObject("httpd");
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var stopImgRes = client.PostAsync(hostAddr + ":9002/worker/stopimage", toSend).Result;

            // Assert
            Assert.IsTrue(stopImgRes.IsSuccessStatusCode);
        }

        [TestMethod]
        public void GetContainerStatusTest()
        {
            // Act
            HttpClient client = new HttpClient();

            // Action
            var status = client.GetAsync(hostAddr + ":9002/worker/getstatus").Result;

            // Assert
            Assert.Equals("exited", status.ToString());
        }

    }
}
