using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.ComponentModel;
using System.Threading;
using System.Xml;
using SharpMule.Automation.Framework.Network; 


namespace SharpMuleAgent
{
   
    public class Agent
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        // A delegate type for hooking up added log notifications.

        static Server server;
        static int port = 5555;
        static void Main(string[] args)
        {
            StartAgentListener(port);
        }

        // Network Agent Listener
        public static void StartAgentListener(int port)
        {
            try
            {
                server = new Server(ResolveHostToIP(), port);
                server.StartListening();
                server.AcceptClientConnections();
            }
            catch
            {
                StartAgentListener(port);
            }
        }


        public static string ResolveHostToIP()
        {
            IPAddress[] hosts = Dns.GetHostAddresses(Dns.GetHostName());
            string hostIp = String.Empty;

            foreach (IPAddress ip in hosts)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIp = ip.ToString();
                }
            }

            return hostIp;
        }
    }
}
