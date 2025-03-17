using System;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Text;
using System.IO;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace BirdCase
{
    public class IpManager : Singleton<IpManager>
    {
        public const int PORT = 7777;
        
        private UnityTransport transport;
        private string localIp;
        private ushort port;
        
        protected override void OnAwake()
        {
        }

        private void Start()
        {
            transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        }
        
        public string GetIp()
        {
            localIp = Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(
                    f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .ToString();
            return localIp;
        }

        public string GetIpAndPort()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(localIp);
            sb.Append(':');
            sb.Append(port);
            return sb.ToString();
        }

        public void HostWithIp(ushort port = PORT)
        {
            this.port = port;
            transport.SetConnectionData(localIp, port);
            NetworkManager.Singleton.StartHost();
        }

        public void JoinWithIp(string ip, ushort port = PORT)
        {
            this.port = port;
            transport.SetConnectionData(ip, port);
            ConnectionManager.Instance.SetConnectionData();
            NetworkManager.Singleton.StartClient();
            ConnectionManager.Instance.WaitForConnect();
        }

        public void SplitIpAndPort(string text, out string ip, out ushort port)
        {
            if (string.Equals(text, ""))
            {
                ip = localIp;
                port = PORT;
                return;
            }
            
            string[] ipAndPort = text.Split(':');

            if (ipAndPort.Length >= 2)
            {
                ip = CheckLocalHost(ipAndPort[0]) ? localIp : ipAndPort[0];
                port = ushort.Parse(ipAndPort[1]);
            }
            else
            {
                if (ipAndPort[0].Contains('.'))
                {
                    ip = CheckLocalHost(ipAndPort[0]) ? localIp : ipAndPort[0];
                    port = PORT;
                }
                else
                {
                    ip = localIp;
                    port = CheckLocalHost(ipAndPort[0]) ? (ushort)PORT : ushort.Parse(ipAndPort[0]);
                }
            }
        }

        private bool CheckLocalHost(string text)
        {
            return text.Equals("127.0.0.1") || text.Equals("") || text.Equals("localhost", StringComparison.OrdinalIgnoreCase);
        }
    }
}
