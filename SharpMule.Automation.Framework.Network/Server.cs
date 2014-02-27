using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using SharpMule.Automation.Framework.Essentials; 

namespace SharpMule.Automation.Framework.Network
{
    [Serializable]
    public class Server
    {
        //Events and Delegates
        public delegate void SuiteCompleteHandler(object sender, EventArgs e, List<TestInterfaceEngine> tlList,TestType type);
        public delegate void TestCompleteHandler(object sender, EventArgs e, TestInterfaceEngine test);
        public event SuiteCompleteHandler SuiteCompleted;
        public event TestCompleteHandler TestCompleted;

        //enum for test type
        public enum TestType { Test, Suite }; 
        // Network vars
        NetLogger Log;
        TcpListener tcpListener;
        TcpClient client;
        NetworkStream strm;

        string ipAddress;
        int port;

        List<TestInterfaceEngine> CompleteTests { get; set; }

        public Server(string ipaddress, int port)
        {
            this.ipAddress = ipaddress;
            this.port = port;
            tcpListener = new TcpListener(IPAddress.Parse(ipaddress), port);
            Log = new NetLogger();
        }

        public void StartListening()
        {
            tcpListener.Start();

            Log.LogInfo("The server is ready and waiting for a connection");

        }

        public void AcceptClientConnections()
        {
            while (true)
            {
                //Accept Client Connections
                client = tcpListener.AcceptTcpClient();
                Log.LogInfo("Connection from Client Accepted. IP: " + client.Client.RemoteEndPoint.ToString());
                NetworkStream strm = client.GetStream();
                Log.LogInfo("Waiting for Client Message");

                IFormatter formatter = new BinaryFormatter();

                object obj = null;

                try
                {
                    obj = formatter.Deserialize(strm);

                }
                catch
                {
                  
                }

                if (obj != null)
                {
                    if (obj.GetType() == typeof(List<TestInterfaceEngine>))
                    {
                        List<TestInterfaceEngine> tlTests = (List<TestInterfaceEngine>)obj;
                        Log.LogInfo("Client Message to run Suite has been recieved and formated");
                        ExecuteSuite(tlTests, TestType.Suite);
                    }
                    else if (obj.GetType() == typeof(TestInterfaceEngine))
                    {
                        List<TestInterfaceEngine> tlTest = new List<TestInterfaceEngine>();
                        tlTest.Add((TestInterfaceEngine)obj);
                        Log.LogInfo("Client Message to run Test has been recieved and formated");

                        ExecuteSuite(tlTest, TestType.Test);
                    }
                    else
                    {
                        Log.LogError("Unknow TestType has been past in");
                    }
                }

            }

        }
        private void ExecuteTest(TestInterfaceEngine test)
        {

            TestCompleted += new TestCompleteHandler(Server_TestCompleted);


            using (BackgroundWorker _worker = new BackgroundWorker())
            {
                _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                {
                    Log.LogInfo("Executing Test ID:" + test.InstanceID);
                    test.ScriptExecute();
                };

                _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                {
                    OnTestCompleted(EventArgs.Empty, test);
                    _worker.Dispose();


                };
                _worker.RunWorkerAsync(test);
            }



        }


        void Server_TestCompleted(object sender, EventArgs e, TestInterfaceEngine test)
        {
            // Send TestInterfaceEngine Object back to Client
            TestCompleted -= Server_TestCompleted; 


            try
            {
                Log.LogInfo("Trying to get Data Stream from the Client");
                strm = client.GetStream();
                IFormatter formatter = new BinaryFormatter();
                Log.LogInfo("Sending Data back to the Client");
                formatter.Serialize(strm, test);

            }            
            catch
            {
                Log.LogError("Unable to send data back to client");

            }

            Log.LogInfo("Test execution has finished successfuly");
        }
        private void ExecuteSuite(List<TestInterfaceEngine> tests,TestType type)
        {

            SuiteCompleted += new SuiteCompleteHandler(Server_SuiteCompleted);
            CompleteTests = new List<TestInterfaceEngine>();

            for (int i = 0; i < tests.Count; i++)
            {
                TestInterfaceEngine test = tests[i];

                using (BackgroundWorker _worker = new BackgroundWorker())
                {
                    _worker.DoWork += delegate(object s, System.ComponentModel.DoWorkEventArgs args)
                    {
                        Log.LogInfo("Executing Test ID:" + test.InstanceID);
                        test.ScriptExecute();
                    };

                    _worker.RunWorkerCompleted += delegate(object s, System.ComponentModel.RunWorkerCompletedEventArgs e)
                    {
                        CompleteTests.Add(test);

                        if (CompleteTests.Count == tests.Count)
                        {
                            OnSuiteCompleted(EventArgs.Empty, CompleteTests,type);
                        }
                        _worker.Dispose();


                    };
                    _worker.RunWorkerAsync(test);
                }
            }


        }

        void Server_SuiteCompleted(object sender, EventArgs e, List<TestInterfaceEngine> tlList,TestType type)
        {
            // Send TestInterfaceEngine Object back to Client
            SuiteCompleted -= Server_SuiteCompleted;
            tlList.Sort(delegate(TestInterfaceEngine tl1, TestInterfaceEngine tl2) { return tl1.InstanceID.CompareTo(tl2.InstanceID); });
           
            try
            {
                Log.LogInfo("Trying to get Data Stream from the Client");
                strm = client.GetStream();
                IFormatter formatter = new BinaryFormatter();
                Log.LogInfo("Sending Data back to the Client");

                if(type == TestType.Test)
                    formatter.Serialize(strm, tlList[0]);
                else
                    formatter.Serialize(strm, tlList);
            }
            catch
            {
                Log.LogError("Unable to send data back to client");

            }

            Log.LogInfo("Test execution has finished successfuly");

        }


        protected virtual void OnSuiteCompleted(EventArgs e, List<TestInterfaceEngine> tlList,TestType type)
        {
            if (SuiteCompleted != null)
                SuiteCompleted(this, e, tlList,type);
        }

        protected virtual void OnTestCompleted(EventArgs e, TestInterfaceEngine test)
        {
            if (TestCompleted != null)
                TestCompleted(this, e, test);
        }
        public void CloseAllConnections()
        {
            Log.LogInfo("Attempting to Cloase all Connections");
            try
            {
                strm.Close();
                client.Close();
                tcpListener.Stop();
            }
            catch
            {
                Log.LogError("An Exception has occured while closing all connections");
            }

        }

    }
}
