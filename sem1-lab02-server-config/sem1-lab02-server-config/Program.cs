using System.Net;

namespace Server.Config;

public class Program
{
    public static void Main()
    {
        Server server = GetServer();
        server.IsReady = CheckConfiguration(server, out var problems);
        
        Console.WriteLine("\n\n\n");
        
        AskAboutStartup();
        
        PrintServerStats(server);
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

    /*
     * Генерирует случайное имя для сервера из двух частей
     */
    private static string GenRandomName()
    {
        Random rand = new Random();
        string[] name1 = ["Fantastic", "Magic", "Unbelievable", "Rotten", "Dirty", "Fast", "Cubic"];
        string[] name2 = ["Craft","MC","World","Place","Room","Bucket","Surface"];
        return name1[rand.Next(0, name1.Length)] + ' ' + name2[rand.Next(0, name2.Length)];
    }

    /*
     * Генерирует случайный айпишник
     */
    private static IPAddress GenRandomIp()
    {
        Random rand = new Random();
        byte[] rBytes = new byte[4];
        rand.NextBytes(rBytes);
        IPAddress ip = new IPAddress(rBytes);
        return ip;
    }

    /*
     * Создаёт случайный экземпляр сервера. По умолчанию IsReady = Ready
     */
    private static Server GetServer()
    {
        Random rand = new Random();
        Server server = new Server
        {
            ServerName =  GenRandomName(),
            PlayerCount = (short)rand.Next(short.MaxValue),
            IsPrivate = Convert.ToBoolean(rand.Next(0,2)),
            Ping = (short)rand.Next(1200),
            Ip = GenRandomIp(),
            Port = rand.Next(2,65535),
            ErrorsCount = rand.Next(0,100),
            IsReady = ServerState.Ready
        };
        return server;
    }

    /*
     * Проверяет показатели сервера и выдаёт ServerState для сервера
     */
    public static ServerState CheckConfiguration(short playerCount, bool isPrivate, short ping, int errorsCount, out List<string> output)
    {
        return GetConfiguration(playerCount, isPrivate, ping, errorsCount, out output);
    }

    /*
     * Проверяет показатели сервера и выдаёт ServerState для сервера
     */
    public static ServerState CheckConfiguration(Server server, out List<string> output)
    {
        short playerCount = server.PlayerCount;
        bool isPrivate = server.IsPrivate;
        short ping = server.Ping;
        int errorsCount = server.ErrorsCount;
        return GetConfiguration(playerCount, isPrivate, ping, errorsCount, out output);
    }
    
    /*
     * Проверяет показатели сервера и выдаёт ServerState для сервера.
     * Если пинг <200 и кол-во ошибок <10 - выдаёт Ready
     * Если сервер запаролен или имеет 10-50 ошибок, или 200-500 пинг - выдаёт Warning
     * В остальных случаях, либо если нет ни одного игрока(или их отрицательное кол-во) - выдаёт Error
     */
    private static ServerState GetConfiguration(short playerCount, bool isPrivate, short ping, int errorsCount, out List<string> problems)
    {
        problems = new List<string>();
        ServerState state = ServerState.Error;
        if (playerCount > 0)
        {
            if (errorsCount < 10 && ping < 200) state = ServerState.Ready;
            if (isPrivate || errorsCount is < 50 and >= 10 || ping is < 500 and >= 200)
            {
                state = ServerState.Warning;
                if (isPrivate) problems.Add("Сервер имеет пароль");
                if (errorsCount is < 50 and >= 10) problems.Add("Большое количество ошибок");
                if (ping is < 500 and >= 200) problems.Add("Высокий пинг");
            }
            if (errorsCount > 50 || ping > 500)
            {
                if (errorsCount > 50) problems.Add("Критическое количество ошибок!");
                if (ping > 500) problems.Add("Критический пинг!");
                state = ServerState.Error;
            }
        } else
        {
            state = ServerState.Error;
            problems.Add("Сервер не может быть запущен без игроков");
        }
        return state;
    }
    
    /*
     * Печатает в консолько хар-ки сервера, а так же все пометки о его проблемах
     */
    private static void PrintServerStats(Server server)
    {
        Console.WriteLine($"Name: {server.ServerName}\n" +
                          $"PlCount: {server.PlayerCount}\n" +
                          $"Private: {server.IsPrivate}\n" +
                          $"Ping: {server.Ping}\n" +
                          $"IpAdress: {server.Ip}:{server.Port}\n" +
                          $"Errors: {server.ErrorsCount}\n" +
                          $"Status: {server.IsReady}");
        
        CheckConfiguration(server, out var problems);
        if (problems.Count > 0)
        {
            Console.WriteLine("\nПроблемы:");
            foreach (var problem in problems) Console.WriteLine(problem);
        }
    }

    /*
     * Преобразовывает скучную строку в крутую
     */
    public static string ToCoolString(string str)
    {
        return $"------------------------------|{str}|------------------------------";
    }

    /*
     * Выводит в консоль вопрос о запуске сервера с вариантами ответа Y/N.
     * Имеет защиту от неправильного ввода.
     * В случае с отрицательным ответом завершает выполнение программы.
     */
    private static void AskAboutStartup()
    {
        while (true)
        {
            Console.Write("Хотите запустить сервер?(Y/N): ");
            var ans = Console.ReadKey().Key;
            if (ans == ConsoleKey.Y)
            {
                Console.WriteLine('\n' + ToCoolString("Инициализация сервера") + '\n');
                break;
            }
            if (ans == ConsoleKey.N)
            {
                Environment.Exit(0);
            }
            Console.WriteLine();
        }
    }
}
