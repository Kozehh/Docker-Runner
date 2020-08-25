using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using WorkerObj;

namespace DBManager
{
    public class Database
    {
        private string server;
        private string user;
        private string password;
        private string database;
        private string port;
        private MySqlConnection con;
        //private MySqlCommand cmd;


        public Database()
        {
        }

        public Database(string server, string user, string password, string database, string port)
        {
            this.server = server;
            this.user = user;
            this.password = password;
            this.database = database;
            this.port = port;
        }

        public virtual MySqlConnection getCon()
        {
            return con;
        }

        public void Connect()
        {
            try
            {
                // Port={port};
                string cs = $@"Server={server}; Port={port}; Database={database}; Uid={user}; Pwd={password}";
                con = new MySqlConnection(cs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Error while trying to connect to the database ..");
            }
        }

        public void InitialiseTables()
        {
            try
            {
                con.Open();
                MySqlCommand cmd = new MySqlCommand
                {
                    Connection = con,
                    CommandText =
                    @"CREATE TABLE IF NOT EXISTS workers(id INT PRIMARY KEY AUTO_INCREMENT, workerAdress TEXT, port TEXT, status INT)"
                };

                cmd.ExecuteNonQuery();

                Console.WriteLine("Workers table created");

                cmd.CommandText =
                    @"CREATE TABLE IF NOT EXISTS images(id INT PRIMARY KEY AUTO_INCREMENT, workerAdress TEXT,  image TEXT)";
                cmd.ExecuteNonQuery();

                Console.WriteLine("Images table created");
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
        }

        public void AddWorker(Worker newWorker)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "INSERT INTO workers(workerAdress, port, status) VALUES('" + newWorker.IPAddress +
                                  "','" + newWorker.Ports + "',0)";
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
            
        }

        public void RemoveWorker(Worker worker)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "DELETE FROM workers WHERE workerAdress = '" + worker.IPAddress + "'";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "DELETE FROM images WHERE workerAdress = '" + worker.IPAddress + "'";
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
        }

        public List<Worker> GetWorkers()
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();

                List<Worker> workersList = new List<Worker>();
                cmd.CommandText = "SELECT * FROM workers";
                using MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Worker worker = new Worker();
                    worker.IPAddress = rdr.GetString(1);
                    worker.Ports = rdr.GetString(2);
                    worker.Ports = rdr.GetString(3);
                    workersList.Add(worker);
                }
                con.Close();
                return workersList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
                return null;
            }
        }

        public bool GetStatus(Worker worker)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "SELECT * FROM workers WHERE workerAdress = '" + worker.IPAddress + "'";
                using MySqlDataReader rdr = cmd.ExecuteReader();
                rdr.Read();
                bool isActive = rdr.GetInt32(3) != 0;
                Console.WriteLine(isActive);
                con.Close();
                return isActive;
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
                return false;
            }
            
        }

        public void UpdateStatus(string workerAdress, int isActive)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "UPDATE workers SET status=" + isActive + " WHERE workerAdress = '" + workerAdress +
                                  "'";
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
            
        }

        public void AddImage(string workerAdress, string image)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "INSERT INTO images(workerAdress, image) VALUES('" + workerAdress + "','" + image + "')";
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
        }

        public void RemoveImage(string workerAdress, string image)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                cmd.CommandText = "DELETE FROM images WHERE workerAdress = '" + workerAdress + "' AND image = '" + image + "'";
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
            }
        }

        public List<string> GetImages(string workerAdress)
        {
            MySqlConnection con = getCon();
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = con;
            try
            {
                con.Open();
                List<string> imagesList = new List<string>();

                cmd.CommandText = "SELECT * FROM images WHERE workerAdress = '" + workerAdress + "'";
                using MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    imagesList.Add(rdr.GetString(2));
                }
                
                con.Close();
                return imagesList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                con.Close();
                return null;
            }
        }
    }
}
