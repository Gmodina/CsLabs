namespace sem_1_lab_04_event_log;

public class Program
{
    
    public struct LogEntry
    {
        public DateTime Timestamp;
        public string Level;
        public string Category;
        public string Message;
    }
    
    /// <summary>
    /// Метод достаёт время лога из строки лога
    /// </summary>
    private static DateTime GetLogDateTime(string line)
    {
        int index1 = line.IndexOf(' ');
        int index2 = line.IndexOf(' ', index1 + 1);

        string datetimeStr = line.Substring(0, index2);

        DateTime dateTime = DateTime.Parse(datetimeStr);
        return dateTime;
    }

    /// <summary>
    /// Метод достаёт уровень лога из строки лога
    /// </summary>
    private static string GetLogLevelLevel(string line)
    {
        int index1 = line.IndexOf('[');
        return line.Substring( index1 + 1, line.IndexOf(']') - index1 - 1);
    }

    /// <summary>
    /// Метод достаёт категорию из строки лога
    /// </summary>
    private static string GetLogCategory(string line)
    {
        string line2 = line.Remove(0, line.IndexOf(']')+1);
        int index2  = line2.IndexOf('[') + 1;
        return line2.Substring(index2, line2.IndexOf(']') - index2);
    }

    /// <summary>
    /// Метод достаёт сообщение из строки лога
    /// </summary>
    private static string GetLogMessage(string line)
    {
        int index1 = line.IndexOf(']', line.IndexOf(']') + 1);
        return line.Substring(index1 + 2);
    }

    /// <summary>
    /// Метод парсит массив строк логов в стракты LogEntry
    /// </summary>
    public static List<LogEntry> ParseLog(string[] lines)
    {
        List<LogEntry> logs = new List<LogEntry>();
        foreach (var line in lines)
        {
            LogEntry log = new LogEntry
            {
                Timestamp = GetLogDateTime(line),
                Level = GetLogLevelLevel(line),
                Category = GetLogCategory(line),
                Message = GetLogMessage(line)
            };
            logs.Add(log);
        }
        return logs;
    }

    /// <summary>
    /// Метод собирает все логи по заданной дате
    /// </summary>
    public static List<LogEntry> FilterByDate(List<LogEntry> entries, DateTime date)
    {
        List<LogEntry> logs = new List<LogEntry>();
        foreach (var log in entries)
        {
            if (log.Timestamp.Date == date.Date) logs.Add(log);
        }
        return logs;
    }
    
    
    /// <summary>
    /// Метод собирает все логи по заданному уровню
    /// </summary>
    public static List<LogEntry> FilterByLevel(List<LogEntry> entries, string level)
    {
        List<LogEntry> logs = new List<LogEntry>();
        foreach (var log in entries)
        {
            if (log.Level == level) logs.Add(log);
        }
        return logs;
    }
    
    /// <summary>
    /// Метод собирает все логи по заданной категории
    /// </summary>
    public static List<LogEntry> FilterByCategory(List<LogEntry> entries, string category)
    {
        List<LogEntry> logs = new List<LogEntry>();
        foreach (var log in entries)
        {
            if (log.Category == category) logs.Add(log);
        }
        return logs;
    }
    
    /// <summary>
    /// Метод ищет в логах совпадения с введённым текстом без учёта регистра
    /// </summary>
    public static List<LogEntry> Search(List<LogEntry> entries, string text)
    {
        List<LogEntry> logs = new List<LogEntry>();
        foreach (var log in entries)
        {
            if (log.Message.ToLowerInvariant().Contains(text.ToLowerInvariant())) logs.Add(log);
        }
        return logs;
    }
    
     /// <summary>
     /// Метод считает кол-во строк логов по заданному уровню
     /// </summary>
    public static int CountByLevel(List<LogEntry> entries, string level)
    {
        var logs = FilterByLevel(entries, level);
        return logs.Count;
    }
    
     /// <summary>
     /// Метод возвращает в стринге статус сервера, основываясь на наличии обычных или фатальных ошибок
     /// </summary>
     ///
    public static string GetServerStatus(List<LogEntry> entries)
    {
        int errors = CountByLevel(entries, "Error");
        int fatals = CountByLevel(entries, "Fatal");
        if (fatals > 0) return "КРИТИЧЕСКАЯ ОШИБКА: сервер остановлен";
        if (errors > 0) return "Есть ошибки: требуется проверка";
        return "Сервер работает штатно";
    }
    /// <summary>
    /// Метод экспортирует в файл строки лога из листа
    /// </summary>
    /// <param name="path"> Путь для сохранения файла</param>
    /// <param name="entries"> Лист логов, который должен быть экспортирован</param>
    public static void ExportFiltered(string path, List<LogEntry> entries)
    {
        string[] lines = new string[entries.Count];
        int cnt = 0;
        foreach (var entry in entries)
        {
            string line = $"{entry.Timestamp} [{entry.Level}][{entry.Category}] {entry.Message}";
            lines[cnt] = line;
            cnt++;
        }
        File.WriteAllLines(path, lines);
    }
    
    public static void Main()
    {
        string[] lines = File.ReadAllLines("event_server.log");
        List<LogEntry> logs = ParseLog(lines);
        ExportFiltered("event_server_filtred.log", Search(logs, "Ночные совы"));
    }
}