namespace sem_1_practice_2_log;

class Program
{
   // 2026-09-01 12:16:01.101 [Info][User] User logged in: RavenFox
    // 2026-09-01 12:16:07.893 [Info][Server] Event "Frostfire Rebellion" scheduled to start in 2 hours
    // 2026-09-01 12:18:25.105 [Info][Event] Event started

    public static DateTime GetLogDateTime(string line)
    {
        // 2026-09-01 12:16:01.101 [Info][User] User logged in: RavenFox

        int index1 = line.IndexOf(' ');
        Console.WriteLine(index1);
        int index2 = line.IndexOf(' ', index1+1);
        Console.WriteLine(index2);

        string datetimeStr = line.Substring(0, index2);
        Console.WriteLine(datetimeStr);

        DateTime dateTime = DateTime.Parse(datetimeStr);
        return dateTime;
    }
    
    public struct Log
    {
        public DateTime dateTime;
        public string level;
        public string type;
        public string text;
    }
    

    public static void Main()
    {

        string[] lines = File.ReadAllLines("event_server.log");
        List<Log> logs = new List<Log>();
        
        foreach (string line in lines)
        {
            Log log = new Log
            {
                dateTime = GetLogDateTime(line),
                level = ,
                type = ,
                text = 
            };
            logs.Add(log);
        }

        for (int i = 0; i < lines.Length; i++)
        {
        }

        // sem-1-practice-2-log-parse
    }
}