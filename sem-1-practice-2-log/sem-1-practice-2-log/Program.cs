namespace sem_1_practice_2_log;

class Program
{
    // 2026-09-01 12:16:01.101 [Info][User] User logged in: RavenFox
    // 2026-09-01 12:16:07.893 [Info][Server] Event "Frostfire Rebellion" scheduled to start in 2 hours
    // 2026-09-01 12:18:25.105 [Info][Event] Event started

    private static DateTime GetLogDateTime(string line)
    {
        // 2026-09-01 12:16:01.101 [Info][User] User logged in: RavenFox

        int index1 = line.IndexOf(' ');
        int index2 = line.IndexOf(' ', index1+1);

        string datetimeStr = line.Substring(0, index2);

        DateTime dateTime = DateTime.Parse(datetimeStr);
        return dateTime;
    }

    private static string GetLogLevelType(string line)
    {
        return line.Substring(line.IndexOf('[')+1, line.IndexOf(']') - line.IndexOf('[')-1);
    }
    
    private static string GetLogType(string line)
    {
        return line.Substring(line.IndexOf('[', line.IndexOf('[')+1)+1, line.IndexOf(']') - line.IndexOf('['));
    }

    private static string GetLogText(string line)
    {
        int index1 = line.IndexOf(']',  line.IndexOf(']')+1);
        return line.Substring(index1+2);
    }
    
    public struct Log
    {
        public DateTime DateTime;
        public string Level;
        public string Type;
        public string Text;
    }
    

    public static void Main()
    {

        string[] lines = File.ReadAllLines("event_server.log");
        List<Log> logs = new List<Log>();
        
        foreach (string line in lines)
        {
            Log log = new Log
            {
                DateTime = GetLogDateTime(line),
                Level = GetLogLevelType(line),
                Type = GetLogType(line),
                Text = GetLogText(line)
            };
            logs.Add(log);
        }
        
        Console.WriteLine(logs[2].DateTime);
        Console.WriteLine(logs[2].Level);
        Console.WriteLine(logs[2].Type);
        Console.WriteLine(logs[2].Text);
    }
}