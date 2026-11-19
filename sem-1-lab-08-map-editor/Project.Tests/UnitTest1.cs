using sem_1_lab_08_map_editor;

namespace Project.Tests
{
    [TestFixture]
    public class MapTests
    {
        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "MapEditorTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        private string TempFile(string name) => Path.Combine(_tempDir, name);

        // Создаёт карту через публичный API, чтобы _mapWidth/_mapHeight были корректными.
        private Map CreateMap(int width, int height)
        {
            var initPath = TempFile("init_" + Guid.NewGuid().ToString("N") + ".txt");
            var map = new Map(new[] { $"{width}|{height}", new string('#', width) });
            map.CreateNewMap(width, height, initPath);
            return map;
        }

        // ======================== Constructor ========================

        [Test]
        public void Constructor_EmptyArray_DoesNotThrow()
        {
            Assert.That(() => new Map(Array.Empty<string>()), Throws.Nothing);
        }

        [Test]
        public void Constructor_ValidMapFile_DoesNotThrow()
        {
            var mapFile = new[] { "5|5", "#####", "#...#", "#.W.#", "#####" };
            Assert.That(() => new Map(mapFile), Throws.Nothing);
        }

        // ======================== CreateNewMap ========================

        [Test]
        public void CreateNewMap_CreatesFileWithHeader()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(12, 7, path);

            Assert.That(File.Exists(path), Is.True);
            var lines = File.ReadAllLines(path);
            Assert.That(lines[0], Is.EqualTo("12|7"));
        }

        [Test]
        public void CreateNewMap_GeneratesCorrectRowCount()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(6, 5, path);

            var lines = File.ReadAllLines(path);
            Assert.That(lines.Length, Is.EqualTo(6)); // 1 заголовок + 5 строк карты
        }

        [Test]
        public void CreateNewMap_EachRowHasCorrectWidth()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(8, 6, path);

            var lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
                Assert.That(lines[i].Length, Is.EqualTo(8), $"Строка {i} неверной длины");
        }

        [Test]
        public void CreateNewMap_BordersAreWalls()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(10, 10, path);

            var lines = File.ReadAllLines(path);
            var wallRow = new string('#', 10);
            Assert.That(lines[1], Is.EqualTo(wallRow), "верхняя стена");
            Assert.That(lines[lines.Length - 1], Is.EqualTo(wallRow), "нижняя стена");
            for (int i = 2; i < lines.Length - 1; i++)
            {
                Assert.That(lines[i][0], Is.EqualTo('#'), $"левая стена, строка {i}");
                Assert.That(lines[i][lines[i].Length - 1], Is.EqualTo('#'), $"правая стена, строка {i}");
            }
        }

        [Test]
        public void CreateNewMap_InteriorIsGroundOrWater()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(20, 20, path);

            var lines = File.ReadAllLines(path);
            for (int i = 2; i < lines.Length - 1; i++)
                for (int j = 1; j < lines[i].Length - 1; j++)
                    Assert.That(lines[i][j], Is.EqualTo('.').Or.EqualTo('W'));
        }

        [Test]
        public void CreateNewMap_HeightOne_OnlyTopWall()
        {
            var map = new Map(new[] { "5|5", "#####" });
            var path = TempFile("out.txt");

            map.CreateNewMap(5, 1, path);

            var lines = File.ReadAllLines(path);
            Assert.That(lines.Length, Is.EqualTo(2));
            Assert.That(lines[1], Is.EqualTo("#####"));
        }

        // ======================== RewriteCell ========================

        [Test]
        public void RewriteCell_PositionOverload_ChangesCell()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");

            map.RewriteCell(new Position(5, 5), CellType.Water);
            map.SaveMap(path);

            var lines = File.ReadAllLines(path);
            Assert.That(lines[6][5], Is.EqualTo('W')); // +1 из-за заголовка
        }

        [Test]
        public void RewriteCell_CellOverload_ChangesCell()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");

            var cell = new Cell(new Position(3, 4), CellType.Ground);
            map.RewriteCell(cell, CellType.Wall);
            map.SaveMap(path);

            var lines = File.ReadAllLines(path);
            Assert.That(lines[5][3], Is.EqualTo('#'));
        }

        [Test]
        public void RewriteCell_OnEmptyMap_DoesNotThrow()
        {
            var map = new Map(Array.Empty<string>());
            Assert.That(() => map.RewriteCell(new Position(1, 1), CellType.Water), Throws.Nothing);
        }

        // ======================== PlacePlayer ========================

        [Test]
        public void PlacePlayer_OnGround_Succeeds()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");
            map.SaveMap(path);
            var (px, py) = FindCell(File.ReadAllLines(path), '.');

            Assert.That(() => map.PlacePlayer(new Position(px, py)), Throws.Nothing);

            map.SaveMap(path);
            Assert.That(File.ReadAllLines(path)[py + 1][px], Is.EqualTo('P'));
        }

        [Test]
        public void PlacePlayer_OnWall_Throws()
        {
            var map = CreateMap(10, 10);
            Assert.That(() => map.PlacePlayer(new Position(0, 0)), Throws.ArgumentException);
        }

        [Test]
        public void PlacePlayer_OnWater_Throws()
        {
            var map = CreateMap(10, 10);
            map.RewriteCell(new Position(5, 5), CellType.Water);

            Assert.That(() => map.PlacePlayer(new Position(5, 5)), Throws.ArgumentException);
        }

        [Test]
        public void PlacePlayer_OutOfBounds_Throws()
        {
            var map = CreateMap(10, 10);
            Assert.That(() => map.PlacePlayer(new Position(100, 100)), Throws.ArgumentException);
        }

        [Test]
        public void PlacePlayer_Twice_Throws()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");
            map.SaveMap(path);
            var (px, py) = FindCell(File.ReadAllLines(path), '.');

            map.PlacePlayer(new Position(px, py));

            Assert.That(() => map.PlacePlayer(new Position(px + 1, py)), Throws.TypeOf<Exception>());
        }

        // ======================== MovePlayer ========================

        [Test]
        public void MovePlayer_ToGround_UpdatesMap()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");
            map.SaveMap(path);
            var (px, py) = FindCell(File.ReadAllLines(path), '.');
            map.PlacePlayer(new Position(px, py));

            map.SaveMap(path); // чтобы «P» попал в файл
            var (tx, ty) = FindCell(File.ReadAllLines(path), '.');

            Assert.That(() => map.MovePlayer(new Position(tx, ty)), Throws.Nothing);

            map.SaveMap(path);
            var lines = File.ReadAllLines(path);
            Assert.That(lines[ty + 1][tx], Is.EqualTo('P'), "на новой позиции должен быть игрок");
            Assert.That(lines[py + 1][px], Is.EqualTo('.'), "на старой — снова земля");
        }

        [Test]
        public void MovePlayer_ToWall_Throws()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");
            map.SaveMap(path);
            var (px, py) = FindCell(File.ReadAllLines(path), '.');
            map.PlacePlayer(new Position(px, py));

            Assert.That(() => map.MovePlayer(new Position(0, 0)), Throws.ArgumentException);
        }

        [Test]
        public void MovePlayer_ToWater_Throws()
        {
            var map = CreateMap(10, 10);
            var path = TempFile("save.txt");
            map.SaveMap(path);
            var (px, py) = FindCell(File.ReadAllLines(path), '.');

            map.RewriteCell(new Position(5, 5), CellType.Water);
            map.PlacePlayer(new Position(px, py));

            Assert.That(() => map.MovePlayer(new Position(5, 5)), Throws.ArgumentException);
        }

        // ======================== SaveMap ========================

        [Test]
        public void SaveMap_WritesHeaderAndCorrectNumberOfLines()
        {
            var map = CreateMap(6, 5);
            var path = TempFile("save.txt");

            map.SaveMap(path);

            Assert.That(File.Exists(path), Is.True);
            var lines = File.ReadAllLines(path);
            Assert.That(lines[0], Is.EqualTo("6|5"));
            Assert.That(lines.Length, Is.EqualTo(6));
        }

        [Test]
        public void SaveMap_WritesRowsOfCorrectWidth()
        {
            var map = CreateMap(7, 4);
            var path = TempFile("save.txt");

            map.SaveMap(path);

            var lines = File.ReadAllLines(path);
            for (int i = 1; i < lines.Length; i++)
                Assert.That(lines[i].Length, Is.EqualTo(7));
        }

        // ======================== Cell / GetCellSymbol ========================

        [TestCase(CellType.Wall, '#')]
        [TestCase(CellType.Ground, '.')]
        [TestCase(CellType.Water, 'W')]
        [TestCase(CellType.Player, 'P')]
        public void Cell_GetCellSymbol_ReturnsExpectedChar(CellType type, char expected)
        {
            var cell = new Cell(new Position(0, 0), type);
            Assert.That(cell.GetCellSymbol(), Is.EqualTo(expected));
        }

        // ======================== Position ========================

        [Test]
        public void Position_StoresCoordinates()
        {
            var pos = new Position(3, 7);
            Assert.That(pos.X, Is.EqualTo(3));
            Assert.That(pos.Y, Is.EqualTo(7));
        }

        [Test]
        public void Position_ValueEquality()
        {
            Assert.That(new Position(2, 5), Is.EqualTo(new Position(2, 5)));
        }

        // ======================== State ========================

        [Test]
        public void State_StoresAllFields()
        {
            var cells = new List<Cell> { new Cell(new Position(1, 1), CellType.Wall) };
            var player = new Player(new Position(2, 3));

            var state = new State(42, cells, player);

            Assert.That(state.Id, Is.EqualTo(42));
            Assert.That(state.Cells, Is.SameAs(cells));
            Assert.That(state.Player.Position.X, Is.EqualTo(2));
            Assert.That(state.Player.Position.Y, Is.EqualTo(3));
        }

        // ======================== Helpers ========================

        // Ищет символ на карте. lines[0] — заголовок, поэтому map row Y = lines[Y + 1].
        private static (int x, int y) FindCell(string[] lines, char symbol)
        {
            for (int y = 0; y < lines.Length - 1; y++)
                for (int x = 0; x < lines[y + 1].Length; x++)
                    if (lines[y + 1][x] == symbol)
                        return (x, y);
            return (-1, -1);
        }
    }
}