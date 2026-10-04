namespace sem_1_lab_03_report_generator;

public class Program
{
    private static DateTime GetLogDateTime(string line)
    {
        int index1 = line.IndexOf(' ');
        int index2 = line.IndexOf(' ', index1 + 1);

        string datetimeStr = line.Substring(0, index2);

        DateTime dateTime = DateTime.Parse(datetimeStr);
        return dateTime;
    }

    private static string GetLogLevelType(string line)
    {
        int index1 = line.IndexOf('[');
        return line.Substring( index1 + 1, line.IndexOf(']') - index1 - 1);
    }

    private static string GetLogType(string line)
    {
        string line2 = line.Remove(0, line.IndexOf(']')+1);
        int index2  = line2.IndexOf('[') + 1;
        return line2.Substring(index2, line2.IndexOf(']') - index2);
    }

    private static string GetLogText(string line)
    {
        int index1 = line.IndexOf(']', line.IndexOf(']') + 1);
        return line.Substring(index1 + 2);
    }

    public struct Log
    {
        public DateTime DateTime;
        public string Level;
        public string Type;
        public string Text;
    }

    public static List<Log> GetEventLogs(string[] lines, out List<Log> killLogs)
    {
        List<Log> eventLogs = new List<Log>();
        killLogs = new List<Log>();
        bool isStarted = false;

        foreach (string line in lines)
        {
            Log log = new Log
            {
                DateTime = GetLogDateTime(line),
                Level = GetLogLevelType(line),
                Type = GetLogType(line),
                Text = GetLogText(line)
            };
            
            if (log.Type == "Event" && log.Text.Contains("Событие началось"))
            {
                isStarted = true;
            }
            if (log.Type == "Server" && log.Text.Contains("закрыто"))
            {
                isStarted = false;
                eventLogs.Add(log);
            }
            if (isStarted) eventLogs.Add(log);
            
            if (log.Type == "Statistics" && log.Text.Contains("победил")) killLogs.Add(log);
        }
        return eventLogs;
    }

    private static (Log, Log, Log, Log, Log, int, int) GetReport(List<Log> logs)
    {
        Log winnerLog = new Log();
        Log rewardLog = new Log();
        Log itemLog = new Log();
        Log rewardLog2 = new Log();
        Log eventName = new Log();
        int warnings = 0;
        int errors = 0;
        foreach (var log in logs)
        {
            if (log.Type == "Event" && log.Text.Contains("Событие началось")) eventName = log;
            if (log.Type == "Reward" && log.Text.Contains("объявлены победителями события")) winnerLog = log;
            if (log.Type == "Reward" && log.Text.Contains("получили") && log.Text.Contains("очков события") && !log.Text.Contains("утешительную")) rewardLog = log;
            if (log.Type == "Loot" && log.Text.Contains("получили ивентовый предмет")) itemLog = log;
            if (log.Type == "Reward" && log.Text.Contains("получили") && log.Text.Contains("очков события") && log.Text.Contains("утешительную")) rewardLog2 = log;
            if (log.Level == "Warning") warnings++;
            if (log.Level == "Error") errors++;
        }
        return (winnerLog,rewardLog,itemLog,rewardLog2,eventName,warnings,errors);
    }

    private static string GetWinner(Log log)
    {
        //Ночные совы объявлены победителями события "Восстание Ледяного Пламени"
        string text = log.Text;
        return text.Substring(0, text.IndexOf("объявлены")-1);

    }

    private static string GetMainReward(Log log)
    {
        //Ночные совы получили 2500 очков события
        string text = log.Text;
        int index1 = text.IndexOf("получили")+"получили".Length;
        return text.Substring(index1+1, text.IndexOf("очков") - index1-1);

    }

    private static string GetItem(Log log)
    {
        //Ночные совы получили ивентовый предмет: Сердце Эмберфанга
        string text = log.Text;
        return text.Substring(text.IndexOf(':')+2);
    }

    private static (string,string) GetSecondaryReward(Log log)
    {
        string text = log.Text;
        int index1 = text.IndexOf(':') + 2;
        string reward = text.Substring(index1, text.IndexOf("очков")-index1-1);
        string team = text.Substring(0, text.IndexOf("получили") - 1);
        return (reward, team);
    }

    private static string GetEventName(Log log)
    {
        //Событие началось: Восстание Ледяного Пламени
        string text = log.Text;
        return text.Substring(text.IndexOf(':')+2);
    }

    public static string BuildReport(string[] lines)
    {
        var eventLogs = GetEventLogs(lines, out var killLogs);
        (Log wLog, Log rLog, Log iLog, Log rLog2, Log eNameLog, int warns, int errs) = GetReport(eventLogs);
        string winner = GetWinner(wLog);
        int reward = Convert.ToInt32(GetMainReward(rLog));
        string item = GetItem(iLog);
        (string sReward, string sTeam) = GetSecondaryReward(rLog2);
        string eventName = GetEventName(eNameLog);
    
        string report = $"# Итоги события: {eventName}\n";
        report += $"\nДата: {eNameLog.DateTime.ToShortDateString()}";
        report += $"\nПобедитель: {winner}";
        report += $"\nОчки победителя: {reward}";
        report += $"\nИвентовый предмет: {item}";
        report += $"\nУтешительная награда команде \"{sTeam}\": {Convert.ToInt32(sReward)} очков";
        report += $"\nПредупреждений во время события: {warns}";
        report += $"\nОшибок во время события: {errs}";
    
        // Дополнительное задание
        int bestScore = 0;
        string bestPlayer = " ";
        foreach (var log in killLogs)
        {
            string text = log.Text;
            int index1 = text.IndexOf(' ');
            int index2 = text.IndexOf("победил") + "победил".Length;
            int curScore = Convert.ToInt32(text.Substring(index2, text.IndexOf("врагов") - index2 - 1));
            if (curScore > bestScore)
            {
                bestScore = curScore;
                bestPlayer = text.Substring(index1 + 1, text.IndexOf("победил") - index1 - 2);
            }
        }
    
        report += $"\nЛучший игрок: {bestPlayer}, победил {bestScore} врагов";
        
    
        return report;
    }


    public static void Main()
    {
        string[] lines = File.ReadAllLines("event_server.log");
       Console.WriteLine(BuildReport(lines));
    }
}