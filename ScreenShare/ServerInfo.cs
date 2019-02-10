using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScreenShare
{
    public class ServerInfo
    {
        static readonly public string Path = (Application.StartupPath + "/www");

        static public string User { get; set; }

        static public string Password { get; set; }

        static public string Ip { get; set; } = "127.0.0.1";

        static public string Port { get; set; } = "8085";

        static public bool IsWorking { get; set; }

        static public List<Tuple<string, string>> IpList { get; set; } = new List<Tuple<string, string>>();

        static StringBuilder sb = new StringBuilder();

        static public string GetInfos()
        {
            sb.Clear();

            if(IsPrivate())
                sb.Append(User).Append(":").Append(Password).Append("@");

            return sb.Append(Ip).Append(":").Append(Port).ToString();
        }

        static public string GetUrl()
        {
            sb.Clear();
            return sb.Append("http://").Append(Ip).Append(":").Append(Port).Append("/").ToString();
        }

        static public string GetUrl2()
        {
            sb.Clear();
            return sb.Append("http://").Append(Environment.MachineName).Append(":").Append(Port).Append("/").ToString();
        }

        static public bool IsPrivate()
        {
            if(!String.IsNullOrEmpty(User) && !String.IsNullOrEmpty(Password))
            {
                return (User.Length > 2 && Password.Length > 3);
            }
            return false;
        }

        static public List<Tuple<string, string>> GetIPv4Addresses()
        {
            IpList.Clear();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                    && ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ua in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IpList.Add(Tuple.Create(ni.Name, ua.Address.ToString()));
                        }
                    }
                }
            }

            return IpList;
        }

    }
}
