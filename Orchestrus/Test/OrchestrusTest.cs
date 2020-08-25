using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DBManager.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WorkerObj;


namespace Test
{
    [TestClass]
    public class OrchestrusTest
    {
        public static string serverAddr = "http://localhost:9000";
        string hostAddr = GetLocalIPAddress();

        public Worker worker = new Worker()
        {
            IPAddress = GetLocalIPAddress(),
            Ports = "7000:80",
            ExeImages = new List<string> { "httpd" }
        };

        [TestMethod]
        public void StartImage()
        {
            // Act
            // Context creation + test
            HttpClient client = new HttpClient();
            // Action
            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var postRest = client.PostAsync(serverAddr + "/api/startimage", toSend).Result;

            // Assert
            // Verification
            Assert.IsTrue(postRest.IsSuccessStatusCode);
        }


        [TestMethod]
        public void StopImage()
        {
            // Act
            // Context creation + test
            HttpClient client = new HttpClient();
            // Action
            var serialize = JsonConvert.SerializeObject(worker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var postRest = client.PostAsync(serverAddr + "/api/stopimage", toSend).Result;

            // Assert
            // Verification
            Assert.IsTrue(postRest.IsSuccessStatusCode);
        }

        [TestMethod]
        public void ShowWorker()
        {
            // Act
            // Context creation + test
            HttpClient client = new HttpClient();
            // Action
            var returnValue = client.GetAsync(serverAddr + "/api/showworker").Result;
            // Assert
            // Verification
            Assert.IsTrue(returnValue.IsSuccessStatusCode);
        }

        [TestMethod]
        public void ShowImages()
        {

            // Act
            // Context creation + test
            HttpClient client = new HttpClient();
            // Action
            //post
            var serialize = JsonConvert.SerializeObject(worker.IPAddress);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var postRest = client.PostAsync(serverAddr + "/api/getimages", toSend).Result;
            //get
            var returnValue = client.GetAsync(serverAddr + "/api/showimages").Result;
            // Assert
            // Verification
            Assert.IsTrue(postRest.IsSuccessStatusCode);
            Assert.IsTrue(returnValue.IsSuccessStatusCode);
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
