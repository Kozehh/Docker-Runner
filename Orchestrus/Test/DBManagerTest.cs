using System;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using WorkerObj;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using DBManager;


namespace Test
{
    [TestClass]
    public class DBManagerTest:Database
    {
        public Worker worker = new Worker()
        {
            IPAddress = "192.168.0.1",
            Ports = "8000:8000",
            ExeImages = new List<string> { "httpd" }
        };

        public static string dbManAddr = "http://localhost:9001";

        public override MySqlConnection getCon()
        {
            string cs = $@"Server={GetLocalIPAddress()}; Port=3309; Database=mydatabase; Uid=root; Pwd=password";
            MySqlConnection con = new MySqlConnection(cs);
            return con;
        }

        public  MySqlConnection initialiseTestsTables()
        {
            string cs = $@"Server={GetLocalIPAddress()}; Port=3309; Database=mydatabase; Uid=root; Pwd=password";
            MySqlConnection con = new MySqlConnection(cs);
            con.Open();
            MySqlCommand cmd = new MySqlCommand("DROP TABLE IF EXISTS workers", con);
            cmd.ExecuteNonQuery();

            cmd.CommandText =
                @"CREATE TABLE workers(id INT PRIMARY KEY AUTO_INCREMENT, workerAdress TEXT, port TEXT, status INT)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "DROP TABLE IF EXISTS images";
            cmd.ExecuteNonQuery();

            cmd.CommandText =
                @"CREATE TABLE images(id INT PRIMARY KEY AUTO_INCREMENT, workerAdress TEXT,  image TEXT)";
            cmd.ExecuteNonQuery();
            con.Close();
            return con;
        }

        public string getWorkerIPFromDB(MySqlConnection con, MySqlCommand cmd, string ipAdr)
        {
            con.Open();
            List<Worker> workersList = new List<Worker>();
            cmd.CommandText = "SELECT * FROM workers WHERE workerAdress = '" + ipAdr + "'";
            using MySqlDataReader rdr = cmd.ExecuteReader();
            string workerAdr = "";
            if (rdr.Read()){
                workerAdr = rdr.GetString(1);
            }
            con.Close();
            return workerAdr;
        }

        public int getWorkerStatusFromDB(MySqlConnection con, MySqlCommand cmd, string ipAdr)
        {
            con.Open();
            List<Worker> workersList = new List<Worker>();
            cmd.CommandText = "SELECT * FROM workers WHERE workerAdress = '" + ipAdr + "'";
            using MySqlDataReader rdr = cmd.ExecuteReader();
            int workerAdr = 0;
            if (rdr.Read()){
                workerAdr = rdr.GetInt32(3);
            }
            con.Close();
            return workerAdr;
        }

        public List<string> getWorkerImageFromDB(MySqlConnection con, MySqlCommand cmd, string ipAdr)
        {
            con.Open();
            List<string> imagesList = new List<string>();

            cmd.CommandText = "SELECT * FROM images WHERE workerAdress = '" + ipAdr + "'";
            using MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                imagesList.Add(rdr.GetString(2));
            }
            
            con.Close();
            return imagesList;
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

        
        [TestMethod]
        public void AddWorker()
        {
            // Create the worker to add
            Worker newWorker = worker;

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
        
            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            var serialize = JsonConvert.SerializeObject(newWorker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRest = client.PostAsync(dbManAddr + "/dbmanager/addworker", toSend).Result;

            //Look in db if worker is there
            string workerAdr = getWorkerIPFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(string.Equals(workerAdr, "192.168.0.1"));
        }

        [TestMethod]
        public void removeworker()
        {
            // Create the worker to remove
            Worker newWorker = worker;

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;


            //Add worker to db
            con.Open();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker.IPAddress +
                                "','" + newWorker.Ports + "',0)";
            cmd.ExecuteNonQuery();
            con.Close();

            //Verifie that it exist in db
            string workerAdr = getWorkerIPFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(String.Equals(workerAdr, "192.168.0.1"));

            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            var serialize = JsonConvert.SerializeObject(newWorker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRest = client.PostAsync(dbManAddr + "/dbmanager/removeworker", toSend).Result;

            //Look in db if worker is there
            workerAdr = getWorkerIPFromDB(con,cmd, newWorker.IPAddress);
            Trace.WriteLine(workerAdr);
            Assert.IsTrue(string.Equals(workerAdr, ""));
        }

        [TestMethod]
        public void startimages()
        {
            // Create the image to start
            Worker newWorker = worker;

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;

            //Add worker to db
            con.Open();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker.IPAddress +
                                "','" + newWorker.Ports + "',0)";
            cmd.ExecuteNonQuery();
            con.Close();
            
            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            var serialize = JsonConvert.SerializeObject(newWorker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRest = client.PostAsync(dbManAddr + "/dbmanager/startimages", toSend).Result;

            //verefie if the image is in the bd and if the status of worker has changed
            int workerStatus = getWorkerStatusFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(workerStatus == 1);
            List<string> workerList = getWorkerImageFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(workerList.Count == 1);
        }

        [TestMethod]
        public void stopimages()
        {
            // Create the image to start
            Worker newWorker = worker;

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;

            //Add worker and image to db
            con.Open();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker.IPAddress +
                                "','" + newWorker.Ports + "',0)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO images(workerAdress, image) VALUES('" + newWorker.IPAddress + "','" + newWorker.ExeImages[0] + "')";
            cmd.ExecuteNonQuery();
            con.Close();
            
            //verifie that the image is in db
            List<string> workerList =getWorkerImageFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(workerList.Count == 1);

            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            var serialize = JsonConvert.SerializeObject(newWorker);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRest = client.PostAsync(dbManAddr + "/dbmanager/stopimages", toSend).Result;

            //verefie if the image is not in the bd and if the status of worker has changed
            int workerStatus = getWorkerStatusFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(workerStatus == 0);
            workerList =getWorkerImageFromDB(con,cmd, newWorker.IPAddress);
            Assert.IsTrue(workerList.Count == 0);
        }

        [TestMethod]
        public void getworkers()
        {
            // Create the workers to start
            Worker newWorker1 = worker;
            Worker newWorker2 = worker;
            newWorker2.IPAddress = "192.168.0.2";

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;

            //Add workers to db
            con.Open();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker1.IPAddress +
                                "','" + newWorker1.Ports + "',0)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker2.IPAddress +
                                "','" + newWorker2.Ports + "',0)";
            cmd.ExecuteNonQuery();
            con.Close();

            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            var returnValue = client.GetAsync(dbManAddr + "/dbmanager/getworkers").Result;
            var content = returnValue.Content.ReadAsStringAsync().Result;
            var workers = JsonConvert.DeserializeObject<List<Worker>>(content);
            //look if we get the workers
            Assert.IsTrue(workers.Count == 2);
        }

        [TestMethod]
        public void getimages()
        {
            // Create the image to start
            Worker newWorker = worker;

            //Connect to db and create new tables
            MySqlConnection con = initialiseTestsTables();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;

            //Add image to db
            con.Open();
            cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker.IPAddress +
                                "','" + newWorker.Ports + "',0)";
            cmd.ExecuteNonQuery();
            cmd.CommandText = "INSERT INTO images(workerAdress, image) VALUES('" + newWorker.IPAddress + "','" + newWorker.ExeImages[0] + "')";
            cmd.ExecuteNonQuery();
            con.Close();

            //Make rest call to DBManager
            HttpClient client = new HttpClient();
            //1.post the worker
            var serialize = JsonConvert.SerializeObject(newWorker.IPAddress);
            var toSend = new StringContent(serialize, Encoding.UTF8, "application/json");
            var startImgRest = client.PostAsync(dbManAddr + "/dbmanager/getimages", toSend).Result;
            // get his image
            var returnValue = client.GetAsync(dbManAddr + "/dbmanager/showimages").Result;
            var content = returnValue.Content.ReadAsStringAsync().Result;
            var images = JsonConvert.DeserializeObject<List<string>>(content);
            Trace.WriteLine(images.Count);
            Assert.IsTrue(images.Count == 1);
        }
    }
}
