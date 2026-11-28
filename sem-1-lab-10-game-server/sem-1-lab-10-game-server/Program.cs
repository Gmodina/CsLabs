using System.Text;
using System.Diagnostics;

namespace sem_1_lab_10_game_server;

public class Program
{
    public static void Main()
    {
        var server = new Server();
        var sergey = new Player("Seryoga");
        var fr = new FileReader("players.csv");
        
        server.ConnectPlayer();
        server.ConnectPlayer();
        server.ConnectPlayer();
        server.ConnectPlayer(sergey);
        server.ConnectPlayers(fr.PlayersList);
        
        var allPlrs = server.GetAllPlayers();
        Console.WriteLine(allPlrs);
        
        // MeasureTheTime();
        
        server.CreateRoom();
        Console.WriteLine(server.RoomsToString());
        server.Rooms[0].ConnectPlayer(sergey);
        Console.WriteLine(server.Rooms[0].PlayersToString());
        server.CreateRoom();
        server.Rooms[1].ConnectPlayer(sergey);
        Console.WriteLine(server.RoomsToString());
        Console.WriteLine(sergey.CurrentRoom?.Id);
    }

    private static void MeasureTheTime()
    {
        var stopwatch = Stopwatch.StartNew();
        var server = new Server();
        var fr = new FileReader("players.csv");
        server.ConnectPlayers(fr.PlayersList);
        
        for (var i = 0; i < 10000; i++)
        {
            // server.GetPlayer("OldMiner"); //4,943 мс
            server.PlayersList.Find(x => x.Login == "OldMiner"); // 5,3594 мс
        }

        stopwatch.Stop();

        Console.WriteLine($"Время поиска: {stopwatch.Elapsed.TotalMilliseconds} мс");
    }
}

public class Player(string login, int level = 1, int health = 10, bool isOnline = false)
{
    public string Login { get; } = login;
    public int Level { get; private set; } = level;
    public int Health { get; private set; } = health;
    public bool IsOnline { get; private set; } = isOnline;
    public ServerRoom? CurrentRoom { get; private set; }

    public void ChangeHealth(int health)
    {
        Health = Math.Max(health+Health, 0);
    }
    
    public void ChangeLevel(int level)
    {
        Level = Math.Max(level+Level, 0);
    }
    
    public void ChangeOnlineStatus(bool online)
    {
        // if (online == IsOnline) throw new ArgumentException($"Player is already {(online ? "online":"offline")}");
        IsOnline = online;
    }

    public void ChangeRoom(ServerRoom? room)
    {
        CurrentRoom = room;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Player: {Login}");
        sb.AppendLine($"Level: {Level}");
        sb.AppendLine($"Health: {Health}");
        sb.AppendLine($"IsOnline: {IsOnline}");
        sb.AppendLine($"CurrentRoom: {CurrentRoom?.Id}");
        return sb.ToString();
    }
}

public class Server
{
    public List<Player> PlayersList { get;} = [];
    public Dictionary<string, Player> PlayersDict { get;} = [];
    public List<ServerRoom> Rooms { get;} = [];
    
    private readonly Random _rnd = new();

    public void ConnectPlayer(Player player)
    {
        if (PlayersDict.TryGetValue(player.Login, out var value))
        {
            value.ChangeOnlineStatus(true);
        }
        else
        {
            player.ChangeOnlineStatus(true);
            PlayersList.Add(player);
            PlayersDict.Add(player.Login, player);
        }
    }

    public void ConnectPlayer()
    {
        Player pl;
        while (true)
        {
            pl = new($"User{_rnd.Next(1, 10000):D5}", _rnd.Next(1, 101),
                _rnd.Next(10, 10000), true);
            if (!PlayersDict.ContainsKey(pl.Login)) break;
        }
        
        PlayersList.Add(pl);
        PlayersDict.Add(pl.Login, pl);
    }

    public void DisconnectPlayer(Player player)
    {
        if (!PlayersDict.TryGetValue(player.Login, out var value))
            throw new ArgumentException($"Player {player.Login} isn't connected");
        value.ChangeOnlineStatus(false);
    }
    
    public void DisconnectPlayer(string login)
    {
        if (!PlayersDict.TryGetValue(login, out var value)) 
            throw new ArgumentException($"Player {login} isn't connected");
        value.ChangeOnlineStatus(false);
    }

    public string GetAllPlayers()
    {
        var sb = new StringBuilder();
        PlayersList.ForEach(x => sb.AppendLine(x.ToString()));
        return sb.ToString();
    }

    public Player GetPlayer(string login)
    {
        if (!PlayersDict.TryGetValue(login, out var value)) 
            throw new ArgumentException($"Player {login} isn't exist");
        return value;
    }

    public enum CharacType
    {
        Health, Level, IsOnline
    }

    public void ChangePlayerCharac(string login, CharacType characType, int value)
    {
        var player = GetPlayer(login);
        switch (characType)
        {
            case CharacType.Health:
                player.ChangeHealth(value);
                break;
            case  CharacType.Level:
                player.ChangeLevel(value);
                break;
            case CharacType.IsOnline:
                player.ChangeOnlineStatus(value != 0);
                break;
            default:
                throw new ArgumentException($"Unknown char type: {characType}");
        }
    }

    public string PlayerToString(string login)
    {
        return !PlayersDict.TryGetValue(login, out var player) ?
            throw new ArgumentException($"Player {login} isn't exist") : player.ToString();
    }

    public void ConnectPlayers(List<Player> players)
    {
        foreach (var player in players)
        {
            ConnectPlayer(player);
        }
    }

    public void CreateRoom()
    {
        var newRoom = new ServerRoom(this);
    }

    public void ConnectRoom(ServerRoom room)
    {
        try
        {
            Rooms.Add(room);
        }
        catch(Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void MovePlayerToRoom(Player player, ServerRoom room)
    {
        try
        {
            player = PlayersDict[player.Login];
            player.CurrentRoom.DisconnectPlayer(player);
            room.ConnectPlayer(player);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public string RoomsToString()
    {
        StringBuilder sb = new();
        foreach (var room in Rooms)
        {
            sb.AppendLine($"id: {room.Id}");
        }
        return sb.ToString();
    }
}

public class FileReader
{
    public List<Player> PlayersList { get;} = [];
    public Dictionary<string, Player> PlayersDict { get;} = [];

    public FileReader(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        foreach (var line in lines)
        {
            string[] split = line.Split('|');
            try
            {
                var player = new Player(split[0],
                    int.Parse(split[1]),
                    int.Parse(split[2]),
                    Convert.ToBoolean(split[3]));
                PlayersList.Add(player);
                PlayersDict.Add(player.Login, player);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

public class ServerRoom
{
    public List<Player> PlayersList { get;} = [];
    public Dictionary<string, Player> PlayersDict { get;} = [];
    public string Id { get; }
    public Server CurrentServer { get; }

    public ServerRoom(Server currentServer)
    {
        Id = Guid.NewGuid().ToString();
        CurrentServer = currentServer;
        CurrentServer.ConnectRoom(this);
    }
    
    public void ConnectPlayer(Player player)
    {
        if (CurrentServer.PlayersDict.TryGetValue(player.Login, out var value))
        {
            if (PlayersDict.TryAdd(player.Login, value))
            {
                PlayersList.Add(value);
                value.ChangeRoom(this);
            }
            else
            {
                throw new InvalidOperationException($"Player {player.Login} is already connected to this ServerRoom");
            }
        }
        else
        {
            throw new InvalidOperationException($"Player {player.Login} isn't exist on server");
        }
    }

    public void DisconnectPlayer(Player player)
    {
        try
        {
            PlayersDict.Remove(player.Login);
            PlayersList.Remove(player);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        player.ChangeRoom(null);
    }
    
    public string PlayersToString()
    {
        StringBuilder sb = new();
        PlayersList.ForEach(x => sb.AppendLine(x.ToString()));
        return sb.ToString();
    }
}