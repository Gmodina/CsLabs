using System.Text;

namespace sem_1_lab_08_map_editor;



public class Program
{
    
    public static void Main()
    {
        Console.CursorVisible = false;

        string[] mapFile = File.ReadAllLines("map.txt");
        
        Map map = new Map(mapFile);
        
        // map.CreateNewMap(20, 20, "map2.txt");
        
        map.RewriteCell(new Position(3, 1), CellType.Water);
        Console.Clear();
        map.PlacePlayer(new Position(1,2));
        map.PrintMap();
    }
}


public class Map
{
    private readonly Random _rnd = new();
    private List<Cell> _cells = [];
    private Player _player;
    private string[] _mapFile;

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
        _cells = [];
        ScanMap(File.ReadAllLines(newMapPath));
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

    private void ScanMap(string[] mapFile)
    {
        if (mapFile.Length == 0) throw new ArgumentException("Map file is empty");
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
    }

    public Cell GetCell(Position pos)
    {
        foreach (var cell in _cells)
        { 
            if (cell.Position.X == pos.X && cell.Position.Y == pos.Y) return cell;
        }
        return default;
    }

    public void PlacePlayer(Position pos)
    {
        var posCellType = GetCell(pos).Type;
        
        //проверка на существование егрока
        
        if (posCellType == CellType.Ground) RewriteCell(pos, CellType.Player);
        else throw new ArgumentException($"Invalid position ({pos.X},{pos.Y}) there is {posCellType}");
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
    
    public void Move(Position pos)
    {
        //удалять прошлого
        Position = pos;
    }
    
}