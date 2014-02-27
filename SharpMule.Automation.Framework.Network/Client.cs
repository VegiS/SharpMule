using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Collections.Generic;
using SharpMule.Automation.Framework.Essentials; 


namespace SharpMule.Automation.Framework.Network
{
    [Serializable]
    public class Client
    {

        string ip;
        int port;
        NetLogger Log;
        

        public Client(string serverName,int port)
        {
            ip = ResolveHostToIP(serverName); 
            this.port = port;
            Log = new NetLogger();


        }
        public Client(long serverIP,int port)
        {
            ip = serverIP.ToString();
            this.port = port;
            Log = new NetLogger();
            
        }

        public bool CanConnect()
        {
            bool isConnected = false; 
            try
            {
                TcpClient client = new TcpClient(ip, port);
                isConnected = client.Connected; 
            }
            catch
            {
                return false; 
            }

           return isConnected; 
        }

        public string ResolveHostToIP(string hostName)
        {
            try
            {
                IPAddress[] hosts = Dns.GetHostAddresses(hostName);
           
                string hostIp = String.Empty; 

                foreach(IPAddress ip in hosts)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                            hostIp = ip.ToString(); 
                    }
                }

                return hostIp; 
            }
            catch (Exception ex)
            {
               Log.LogError("An Exception has occured getting the hostname address: " + ex.Message); 
               return String.Empty;

            }
           
        }
        public List<TestInterfaceEngine> SendTLObject(List<TestInterfaceEngine> ListOfTests)
        {

                TcpClient client = new TcpClient(this.ip, this.port);

                IFormatter formatter = new BinaryFormatter();
                NetworkStream strm = client.GetStream();

                if (client.Connected)
                    Log.LogInfo("Client is connected to server agent and ready to work");
                else
                    Log.LogError("Client was unable to connect to server agent"); 
            
                formatter.Serialize(strm, ListOfTests);

                Log.LogInfo("Sending TL Object to server agent"); 

                strm = client.GetStream();


                ListOfTests = (List<TestInterfaceEngine>)formatter.Deserialize(strm);
                Log.LogInfo("Received TL Object back from server");

                return ListOfTests; 
            
        }

        public TestInterfaceEngine SendTLObject(TestInterfaceEngine test)
        {

            TcpClient client = new TcpClient(this.ip, this.port);

            IFormatter formatter = new BinaryFormatter();
            NetworkStream strm = client.GetStream();

            if (client.Connected)
                Log.LogInfo("Client is connected to server agent and ready to work");
            else
                Log.LogError("Client was unable to connect to server agent");

            formatter.Serialize(strm, test);

            Log.LogInfo("Sending TL Object to server agent");

            strm = client.GetStream();


            test = (TestInterfaceEngine)formatter.Deserialize(strm);
            Log.LogInfo("Received TL Object back from server");

            return test;

        }
       
    }
}
