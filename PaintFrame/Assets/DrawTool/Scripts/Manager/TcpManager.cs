using System.Linq;
using System.Net;
using UnityEngine;

public class TcpManager : MonoBehaviour {

    private static string hostIP = "127.0.0.1";

    /// <summary>
    /// 获取本机IP
    /// </summary>
    /// <returns></returns>
    private static IPAddress GetIP()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.FirstOrDefault<IPAddress>(a => a.AddressFamily.ToString().Equals("InterNetwork"));
    }

    /// <summary>
    /// 是否在线
    /// </summary>
    /// <returns></returns>
    public static bool IsOnLine()
    {
        return !(Application.internetReachability == NetworkReachability.NotReachable);//检测是否不可连接
    }
}
