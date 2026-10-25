namespace sem_1_lab_06_realtime_monitor;

public class Program
{
    public static double[] _cpuUsage = new double[20];
    public static double[] _ramUsage = new double[20];
    public static double[] _temperature = new double[20];
    public static int _currIndex = 0;
    
    public static void Main()
    {
        var monitor = new ServerMonitor();
        monitor.DataReceived += OnDataReceived;
        monitor.Start();
        Console.WriteLine("Мониторинг запущен.");
        Console.WriteLine("Для завершения программы нажмите Enter.");
        Console.ReadLine();
        monitor.Stop();
    }
    
    public static void OnDataReceived(double cpu, double ram, double temperature)
    {
        //try-catch для юнит-тестов
        try
        {
            Console.Clear();
        }
        catch
        {
            //для юнит-тестов
        }
        
        int arrLength = _cpuUsage.Length-1;

        if (_currIndex == arrLength)
        {
            for (int i = 0; i < arrLength; i++)
            {
                _cpuUsage[i] = _cpuUsage[i+1];
                _ramUsage[i] = _ramUsage[i+1];
                _temperature[i] = _temperature[i+1];
            }
        }
        
        _cpuUsage[_currIndex] = cpu;
        _ramUsage[_currIndex] = ram;
        _temperature[_currIndex] = temperature;
        _currIndex = Math.Min(_currIndex + 1, arrLength);
        
        
        
        Console.WriteLine($"CPU Usage:   {BuildGraph(_cpuUsage)}");
        Console.WriteLine($"RAM Usage:   {BuildGraph(_ramUsage)}");
        Console.WriteLine($"Temperature: {BuildGraph(_temperature)}");
        
    }

    public static string BuildGraph(double[] arr)
    {
        string graph = "";
        foreach (var item in arr)
        {
            if (item >= 75) graph += "█ ";
            if (item is >= 50 and < 75) graph += "▓ ";
            if (item is >= 25 and < 50) graph += "▒ ";
            if (item is >= 0 and < 25) graph += "░ ";
        }
        return graph;
    }
}