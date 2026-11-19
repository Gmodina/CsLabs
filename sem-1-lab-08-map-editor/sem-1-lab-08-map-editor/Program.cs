using System.Text;

namespace sem_1_lab_08_map_editor;



public class Program
{
    
    public static void Main()
    {
        //В тз нет задачи делать интерфейс, а потому я его делать не буду
        Console.CursorVisible = false;

        string[] mapFile = File.ReadAllLines("map.txt");
        
        Map map = new Map(mapFile);
        
        map.CreateNewMap(5, 5, "map2.txt"); //создание новой карты
        
        map.RewriteCell(new Position(3, 1), CellType.Water); //изменение типа отдельных клеток
        Console.Clear();
        map.PlacePlayer(new Position(1,2));//размещение игрока на карте
        map.SaveState();//сохранение текущего состояния локально
        map.MovePlayer(new Position(2,2));//перемещение игрока по карте
        map.PrintMap();//отображение карты в консоли
        map.SaveMap("map3.txt");//сохранение текущего состояния карты в файл
        map.ScanMap(File.ReadAllLines("map3.txt"));//восстановление ранее сохранённого состояния из файла
        map.PrintMap();
        map.LoadState(0);//восстановление состояний локально

        var map2 = new Map(13, 12, "mapp14.txt");
        map2.PrintMap();
    }
}


public class Map
{
    private readonly Random _rnd = new();
    private List<Cell> _cells = []; //можно было бы сделать через двумерный массив
    private Player _player;
    private string[] _mapFile;
    public int MapWidth;
    public int MapHeight;
    private int _currentId = 0;
    private List<State> _states = [];

    public Map(string[] mapFile)
    {
        _mapFile = mapFile;
        try
        {
            ScanMap(_mapFile);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public Map(int width, int height, string newMapPath)
    {
        CreateNewMap(width, height, newMapPath);
    }
    
    public void CreateNewMap(int width, int height, string newMapPath)
    {
        StringBuilder map = new StringBuilder();
        
        map.AppendLine($"{width}|{height}");
        
        string cover = new string('#', width);
        map.AppendLine(cover);
        
        for (int h = 0; h < height-2; h++)
        {
            char[] chars = ("#" + new string('.', width-2) + "#").ToCharArray();

            if (_rnd.Next(0, 2) == 0)
            {
                int rndIndex = _rnd.Next(1, chars.Length - 1);
                if (chars[rndIndex] == '.') chars[rndIndex] = 'W';
            }

            string line = new string(chars);
            map.AppendLine(line);
        }
        
        if (height>1) map.AppendLine(cover);
        
        File.WriteAllText(newMapPath, map.ToString());

        try
        {
            _cells = [];
            ScanMap(File.ReadAllLines(newMapPath));
            _player = new();
            MapHeight = height;
            MapWidth = width;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    public void RewriteCell(Cell oldCell, CellType newType)
    {
        var oldPos = oldCell.Position;
        RewriteCell(oldPos, newType);
    }

    public void RewriteCell(Position oldPos, CellType newType)
    {
        if (_cells.Count == 0) return;
        
        for (int i = 0; i < _cells.Count; i++)
        {
            var cell = _cells[i];
            if (cell.Position.X == oldPos.X && cell.Position.Y == oldPos.Y) cell.Type = newType;
            _cells[i] = cell;
        }
    }

    public void ScanMap(string[] mapFile)
    {
        if (mapFile.Length <= 0) throw new ArgumentException("Map file is empty");
        for (int h = 0; h < mapFile.Length; h++)
        {
            for (int w = 0; w < mapFile[h].Length; w++)
            {
                Position pos = new (w, h-1);
                switch (mapFile[h][w])
                {
                    case '#':
                        _cells.Add(new Cell(pos, CellType.Wall));
                        break;
                    case '.':
                        _cells.Add(new Cell(pos, CellType.Ground));
                        break;
                    case 'W':
                        _cells.Add(new Cell(pos, CellType.Water));
                        break;
                    case 'P':
                        _cells.Add(new Cell(pos, CellType.Player));
                        _player.Position = pos;
                        break;
                }
            }
        }
        MapHeight = mapFile.Length-1;
        MapWidth = mapFile[1].Length;
    }

    private Cell GetCell(Position pos)
    {
        foreach (var cell in _cells)
        { 
            if (cell.Position.X == pos.X && cell.Position.Y == pos.Y) return cell;
        }
        return default;
    }

    public void PlacePlayer(Position pos)
    {
        var targetCellType = GetCell(pos).Type;
        
        if (_player.Position.Y != 0) throw new Exception($"Player already placed at ({_player.Position.X}, {_player.Position.Y})");
        
        if (targetCellType == CellType.Ground)
        {
            RewriteCell(pos, CellType.Player);
            _player.Position = pos;
        }
        else throw new ArgumentException($"Invalid position ({pos.X},{pos.Y}), there is {targetCellType}");
    }

    public void MovePlayer(Position pos)
    {
        var targetCellType = GetCell(pos).Type;

        if (targetCellType == CellType.Ground)
        {
            RewriteCell(new Position(_player.Position.X, _player.Position.Y), CellType.Ground);
            RewriteCell(pos, CellType.Player);
            _player.Position = pos;
        }
        
        else throw new ArgumentException($"Can't move player to ({pos.X},{pos.Y}), there is {targetCellType}");
    }

    public void SaveMap(string saveMapPath)
    {
        StringBuilder sb = new();
        
        sb.AppendLine($"{MapWidth.ToString()}|{MapHeight.ToString()}");

        int i = 0;
        char[] symbols = new char[MapWidth];
        foreach (var cell in _cells)
        {
            symbols[i] = cell.GetCellSymbol();
            i++;
            if (i == MapWidth)
            {
                sb.AppendLine(new string(symbols));
                i = 0;
            }
        }
        File.WriteAllText(saveMapPath, sb.ToString());
    }

    public void SaveState()
    {
        var state = new State(_currentId, _cells, _player);
        _states.Add(state);
        _currentId++;
    }

    public void LoadState(int id)
    {
        if (_states.Count <= id) return;
        _cells = _states[id].Cells;
        _player = _states[id].Player;
    }

    public void PrintMap()
    {
        foreach (var cell in _cells)
        {
            Console.SetCursorPosition(cell.Position.X, cell.Position.Y);
            Console.Write(cell.GetCellSymbol());
        }
    }
}

public struct Position(int x, int y)
{
    public int X = x;
    public int Y = y;
}

public enum CellType
{
    Wall,
    Ground,
    Water,
    Player
}


public struct Cell(Position pos, CellType cellType)
{
    public Position Position = pos;
    public CellType Type = cellType;
    public char GetCellSymbol()
    {
        switch (Type)
        {
            case CellType.Wall:
                return '#';
            case CellType.Ground:
                return '.';
            case CellType.Player:
                return 'P';
            case CellType.Water:
                return 'W';
        }
        return '#';
    }
}

public struct Player(Position pos)
{
    public Position Position = pos;
}

public class State(int id, List<Cell> cells, Player player)
{
    public int Id = id;
    public List<Cell> Cells = cells;
    public Player Player = player;
}