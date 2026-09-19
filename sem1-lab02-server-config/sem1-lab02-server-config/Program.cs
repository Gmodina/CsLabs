using System.Net;

namespace Server.Config;

public class Program
{
    public static void Main()
    {
        Server server = GetServer();
        Console.WriteLine($"{server.ServerName},{server.PlayerCount},{server.IsPrivate},{server.Ping},{server.Ip},{server.Port},{server.ErrorsCount},{server.IsReady}");
    }

    public enum ServerState
    {
        Ready,
        Warning,
        Error
    }
    
    public struct Server
    {
        public string ServerName;
        public short PlayerCount;
        public bool IsPrivate;
        public short Ping;
        public IPAddress Ip;
        public int Port;
        public int ErrorsCount;
        public ServerState IsReady;
    }

    private static string GenRandomName()
    {
        Random rand = new Random();
        string[] name1 = ["Fantastic", "Magic", "Unbelievable", "Rotten", "Dirty", "Fast", "Cubic"];
        string[] name2 = ["Craft","MC","World","Place","Room","Bucket","Surface"];
        return name1[rand.Next(0, name1.Length)] + ' ' + name2[rand.Next(0, name2.Length)];
    }

    private static IPAddress GenRandomIp()
    {
        Random rand = new Random();
        byte[] rBytes = new byte[4];
        rand.NextBytes(rBytes);
        IPAddress ip = new IPAddress(rBytes);
        return ip;
    }

    private static Server GetServer()
    {
        Random rand = new Random();
        Server server = new Server
        {
            ServerName =  GenRandomName(),
            PlayerCount = (short)rand.Next(short.MaxValue),
            IsPrivate = Convert.ToBoolean(rand.Next(0,2)),
            Ping = (short)rand.Next(short.MaxValue),
            Ip = GenRandomIp(),
            Port = rand.Next(2,65535),
            ErrorsCount = rand.Next(0,100),
            IsReady = ServerState.Ready
        };
        return server;
    }
}
