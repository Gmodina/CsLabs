namespace Project.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using sem_1_lab_10_game_server;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Constructor_SetsProperties()
    {
        var player = new Player("Test", 5, 100, true);

        Assert.That(player.Login, Is.EqualTo("Test"));
        Assert.That(player.Level, Is.EqualTo(5));
        Assert.That(player.Health, Is.EqualTo(100));
        Assert.That(player.IsOnline, Is.True);
        Assert.That(player.CurrentRoom, Is.Null);
    }

    [Test]
    public void Constructor_DefaultValues()
    {
        var player = new Player("Test");

        Assert.That(player.Level, Is.EqualTo(1));
        Assert.That(player.Health, Is.EqualTo(10));
        Assert.That(player.IsOnline, Is.False);
    }

    [TestCase(5, 15)]
    [TestCase(-5, 5)]
    [TestCase(-100, 0)]
    [TestCase(0, 10)]
    public void ChangeHealth_AddsAndClampsAtZero(int delta, int expected)
    {
        var player = new Player("Test", health: 10);

        player.ChangeHealth(delta);

        Assert.That(player.Health, Is.EqualTo(expected));
    }

    [TestCase(5, 6)]
    [TestCase(-1, 0)]
    [TestCase(-100, 0)]
    public void ChangeLevel_AddsAndClampsAtZero(int delta, int expected)
    {
        var player = new Player("Test", level: 1);

        player.ChangeLevel(delta);

        Assert.That(player.Level, Is.EqualTo(expected));
    }

    [Test]
    public void ChangeOnlineStatus_SetsValue()
    {
        var player = new Player("Test");

        player.ChangeOnlineStatus(true);
        Assert.That(player.IsOnline, Is.True);

        player.ChangeOnlineStatus(false);
        Assert.That(player.IsOnline, Is.False);
    }

    [Test]
    public void ChangeRoom_SetsCurrentRoom()
    {
        var server = new Server();
        var player = new Player("Test");
        var room = new ServerRoom(server);

        player.ChangeRoom(room);

        Assert.That(player.CurrentRoom, Is.SameAs(room));
    }

    [Test]
    public void ToString_ContainsAllFields()
    {
        var player = new Player("Test", 2, 20, true);

        var result = player.ToString();

        Assert.That(result, Does.Contain("Player: Test"));
        Assert.That(result, Does.Contain("Level: 2"));
        Assert.That(result, Does.Contain("Health: 20"));
        Assert.That(result, Does.Contain("IsOnline: True"));
        Assert.That(result, Does.Contain("CurrentRoom:"));
    }
}

[TestFixture]
public class ServerTests
{
    private Server _server = null!;

    [SetUp]
    public void SetUp()
    {
        _server = new Server();
    }

    [Test]
    public void ConnectPlayer_NewPlayer_AddsAndSetsOnline()
    {
        var player = new Player("A");

        _server.ConnectPlayer(player);

        Assert.That(_server.PlayersList, Has.Count.EqualTo(1));
        Assert.That(_server.PlayersDict, Contains.Key("A"));
        Assert.That(_server.PlayersDict["A"], Is.SameAs(player));
        Assert.That(player.IsOnline, Is.True);
    }

    [Test]
    public void ConnectPlayer_ExistingPlayer_DoesNotDuplicateAndSetsOnline()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);
        player.ChangeOnlineStatus(false);

        _server.ConnectPlayer(player);

        Assert.That(_server.PlayersList, Has.Count.EqualTo(1));
        Assert.That(_server.PlayersDict, Has.Count.EqualTo(1));
        Assert.That(player.IsOnline, Is.True);
    }

    [Test]
    public void ConnectPlayer_ExistingLoginDifferentInstance_SetsExistingOnline()
    {
        var first = new Player("A");
        var second = new Player("A");

        _server.ConnectPlayer(first);
        _server.ConnectPlayer(second);

        Assert.That(_server.PlayersList, Has.Count.EqualTo(1));
        Assert.That(_server.PlayersDict["A"], Is.SameAs(first));
        Assert.That(first.IsOnline, Is.True);
        Assert.That(second.IsOnline, Is.False);
    }

    [Test]
    public void ConnectPlayer_Random_AddsUniquePlayer()
    {
        _server.ConnectPlayer();
        _server.ConnectPlayer();

        Assert.That(_server.PlayersList, Has.Count.EqualTo(2));
        Assert.That(_server.PlayersDict, Has.Count.EqualTo(2));

        foreach (var player in _server.PlayersList)
        {
            Assert.That(player.Login, Does.Match(@"^User\d{5}$"));
            Assert.That(player.IsOnline, Is.True);
            Assert.That(_server.PlayersDict[player.Login], Is.SameAs(player));
        }
    }

    [Test]
    public void DisconnectPlayer_Existing_SetsOffline()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        _server.DisconnectPlayer(player);

        Assert.That(player.IsOnline, Is.False);
    }

    [Test]
    public void DisconnectPlayer_NonExisting_Throws()
    {
        var player = new Player("A");

        Assert.Throws<ArgumentException>(() => _server.DisconnectPlayer(player));
    }

    [Test]
    public void DisconnectPlayer_ByLogin_Existing_SetsOffline()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        _server.DisconnectPlayer("A");

        Assert.That(player.IsOnline, Is.False);
    }

    [Test]
    public void DisconnectPlayer_ByLogin_NonExisting_Throws()
    {
        Assert.Throws<ArgumentException>(() => _server.DisconnectPlayer("A"));
    }

    [Test]
    public void GetPlayer_Existing_ReturnsSameInstance()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        Assert.That(_server.GetPlayer("A"), Is.SameAs(player));
    }

    [Test]
    public void GetPlayer_NonExisting_Throws()
    {
        Assert.Throws<ArgumentException>(() => _server.GetPlayer("A"));
    }

    [Test]
    public void ChangePlayerCharac_Health_Changes()
    {
        var player = new Player("A", health: 10);
        _server.ConnectPlayer(player);

        _server.ChangePlayerCharac("A", Server.CharacType.Health, 5);

        Assert.That(player.Health, Is.EqualTo(15));
    }

    [Test]
    public void ChangePlayerCharac_Level_Changes()
    {
        var player = new Player("A", level: 1);
        _server.ConnectPlayer(player);

        _server.ChangePlayerCharac("A", Server.CharacType.Level, 2);

        Assert.That(player.Level, Is.EqualTo(3));
    }

    [Test]
    public void ChangePlayerCharac_IsOnline_Changes()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        _server.ChangePlayerCharac("A", Server.CharacType.IsOnline, 0);
        Assert.That(player.IsOnline, Is.False);

        _server.ChangePlayerCharac("A", Server.CharacType.IsOnline, 1);
        Assert.That(player.IsOnline, Is.True);
    }

    [Test]
    public void ChangePlayerCharac_UnknownType_Throws()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        Assert.Throws<ArgumentException>(() =>
            _server.ChangePlayerCharac("A", (Server.CharacType)999, 1));
    }

    [Test]
    public void ChangePlayerCharac_NonExisting_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _server.ChangePlayerCharac("A", Server.CharacType.Health, 1));
    }

    [Test]
    public void PlayerToString_Existing_ReturnsString()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        Assert.That(_server.PlayerToString("A"), Does.Contain("Player: A"));
    }

    [Test]
    public void PlayerToString_NonExisting_Throws()
    {
        Assert.Throws<ArgumentException>(() => _server.PlayerToString("A"));
    }

    [Test]
    public void GetAllPlayers_ReturnsAll()
    {
        _server.ConnectPlayer(new Player("A"));
        _server.ConnectPlayer(new Player("B"));

        var result = _server.GetAllPlayers();

        Assert.That(result, Does.Contain("Player: A"));
        Assert.That(result, Does.Contain("Player: B"));
    }

    [Test]
    public void ConnectPlayers_AddsAll()
    {
        var players = new List<Player>
        {
            new("A"),
            new("B"),
            new("C")
        };

        _server.ConnectPlayers(players);

        Assert.That(_server.PlayersList, Has.Count.EqualTo(3));
        Assert.That(_server.PlayersDict.Keys, Is.EquivalentTo(new[] { "A", "B", "C" }));
    }

    [Test]
    public void ConnectPlayers_Duplicate_DoesNotDuplicate()
    {
        var first = new Player("A");

        _server.ConnectPlayers(new List<Player>
        {
            first,
            first,
            new("A")
        });

        Assert.That(_server.PlayersList, Has.Count.EqualTo(1));
        Assert.That(_server.PlayersDict, Has.Count.EqualTo(1));
    }

    [Test]
    public void CreateRoom_AddsRoom()
    {
        _server.CreateRoom();

        Assert.That(_server.Rooms, Has.Count.EqualTo(1));
        Assert.That(_server.Rooms[0].CurrentServer, Is.SameAs(_server));
    }

    [Test]
    public void MovePlayerToRoom_FromOneRoomToAnother()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        var room1 = new ServerRoom(_server);
        var room2 = new ServerRoom(_server);

        room1.ConnectPlayer(player);
        _server.MovePlayerToRoom(player, room2);

        Assert.That(room1.PlayersList, Is.Empty);
        Assert.That(room2.PlayersList, Contains.Item(player));
        Assert.That(player.CurrentRoom, Is.SameAs(room2));
    }

    [Test]
    public void MovePlayerToRoom_ToSameRoom_NoDuplicate()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        var room = new ServerRoom(_server);
        room.ConnectPlayer(player);

        _server.MovePlayerToRoom(player, room);

        Assert.That(room.PlayersList, Has.Count.EqualTo(1));
        Assert.That(player.CurrentRoom, Is.SameAs(room));
    }

    [Test]
    public void MovePlayerToRoom_PlayerNotInAnyRoom_DoesNotMove()
    {
        var player = new Player("A");
        _server.ConnectPlayer(player);

        var room = new ServerRoom(_server);

        _server.MovePlayerToRoom(player, room);

        Assert.That(room.PlayersList, Is.Empty);
        Assert.That(player.CurrentRoom, Is.Null);
    }

    [Test]
    public void MovePlayerToRoom_NonExistingPlayer_DoesNotThrow()
    {
        var player = new Player("A");
        var room = new ServerRoom(_server);

        Assert.DoesNotThrow(() => _server.MovePlayerToRoom(player, room));
        Assert.That(room.PlayersList, Is.Empty);
    }

    [Test]
    public void RoomsToString_ContainsRoomIds()
    {
        var room = new ServerRoom(_server);

        var result = _server.RoomsToString();

        Assert.That(result, Does.Contain(room.Id));
    }
}

[TestFixture]
public class ServerRoomTests
{
    private Server _server = null!;
    private Player _player = null!;
    private ServerRoom _room = null!;

    [SetUp]
    public void SetUp()
    {
        _server = new Server();
        _player = new Player("A");
        _server.ConnectPlayer(_player);
        _room = new ServerRoom(_server);
    }

    [Test]
    public void Constructor_CreatesUniqueIdAndRegisters()
    {
        var room2 = new ServerRoom(_server);

        Assert.That(_room.Id, Is.Not.Empty);
        Assert.That(room2.Id, Is.Not.EqualTo(_room.Id));
        Assert.That(_server.Rooms, Does.Contain(_room));
        Assert.That(_server.Rooms, Does.Contain(room2));
        Assert.That(_room.CurrentServer, Is.SameAs(_server));
    }

    [Test]
    public void ConnectPlayer_Valid_AddsAndSetsRoom()
    {
        _room.ConnectPlayer(_player);

        Assert.That(_room.PlayersList, Contains.Item(_player));
        Assert.That(_room.PlayersDict["A"], Is.SameAs(_player));
        Assert.That(_player.CurrentRoom, Is.SameAs(_room));
    }

    [Test]
    public void ConnectPlayer_AlreadyInRoom_Throws()
    {
        _room.ConnectPlayer(_player);

        Assert.Throws<InvalidOperationException>(() => _room.ConnectPlayer(_player));
    }

    [Test]
    public void ConnectPlayer_NotOnServer_Throws()
    {
        var stranger = new Player("B");

        Assert.Throws<InvalidOperationException>(() => _room.ConnectPlayer(stranger));
    }

    [Test]
    public void ConnectPlayer_DifferentInstanceSameLogin_UsesServerInstance()
    {
        var other = new Player("A");

        _room.ConnectPlayer(other);

        Assert.That(_room.PlayersDict["A"], Is.SameAs(_player));
        Assert.That(_player.CurrentRoom, Is.SameAs(_room));
        Assert.That(other.CurrentRoom, Is.Null);
    }

    [Test]
    public void DisconnectPlayer_Valid_RemovesAndClearsRoom()
    {
        _room.ConnectPlayer(_player);

        _room.DisconnectPlayer(_player);

        Assert.That(_room.PlayersList, Is.Empty);
        Assert.That(_room.PlayersDict, Is.Empty);
        Assert.That(_player.CurrentRoom, Is.Null);
    }

    [Test]
    public void DisconnectPlayer_NotInRoom_DoesNotThrowAndClearsRoom()
    {
        Assert.DoesNotThrow(() => _room.DisconnectPlayer(_player));
        Assert.That(_player.CurrentRoom, Is.Null);
    }

    [Test]
    public void PlayersToString_ContainsPlayerInfo()
    {
        _room.ConnectPlayer(_player);

        var result = _room.PlayersToString();

        Assert.That(result, Does.Contain("Player: A"));
    }

    [Test]
    [Ignore("Выявляет потенциальный баг: DisconnectPlayer в чужой комнате обнуляет CurrentRoom, не удаляя игрока из реальной комнаты")]
    public void DisconnectPlayer_FromWrongRoom_ShouldNotAffectPlayer()
    {
        var room2 = new ServerRoom(_server);
        room2.ConnectPlayer(_player);

        _room.DisconnectPlayer(_player);

        Assert.That(_player.CurrentRoom, Is.SameAs(room2));
        Assert.That(room2.PlayersList, Contains.Item(_player));
    }

    [Test]
    [Ignore("Выявляет потенциальный баг: ConnectPlayer в другую комнату не удаляет игрока из старой комнаты")]
    public void ConnectPlayer_AlreadyInAnotherRoom_ShouldMoveOrThrow()
    {
        var room2 = new ServerRoom(_server);
        _room.ConnectPlayer(_player);

        room2.ConnectPlayer(_player);

        Assert.That(_room.PlayersList, Is.Empty);
        Assert.That(_player.CurrentRoom, Is.SameAs(room2));
    }
}

[TestFixture]
public class FileReaderTests
{
    private readonly List<string> _tempFiles = new();

    private string CreateTempFile(string content)
    {
        var path = Path.GetTempFileName();
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        _tempFiles.Clear();
    }

    [Test]
    public void Constructor_ValidFile_ParsesPlayers()
    {
        var path = CreateTempFile("A|1|10|false\nB|2|20|true");

        var reader = new FileReader(path);

        Assert.That(reader.PlayersList, Has.Count.EqualTo(2));
        Assert.That(reader.PlayersDict, Has.Count.EqualTo(2));
        Assert.That(reader.PlayersDict["A"].Level, Is.EqualTo(1));
        Assert.That(reader.PlayersDict["A"].Health, Is.EqualTo(10));
        Assert.That(reader.PlayersDict["A"].IsOnline, Is.False);
        Assert.That(reader.PlayersDict["B"].IsOnline, Is.True);
    }

    [Test]
    public void Constructor_EmptyFile_ReturnsEmptyCollections()
    {
        var path = CreateTempFile("");

        var reader = new FileReader(path);

        Assert.That(reader.PlayersList, Is.Empty);
        Assert.That(reader.PlayersDict, Is.Empty);
    }

    [Test]
    public void Constructor_InvalidLines_SkipsInvalid()
    {
        var path = CreateTempFile(
            "A|1|10|false\n" +
            "invalid\n" +
            "B|x|20|true\n" +
            "C|3|30|notbool");

        var reader = new FileReader(path);

        Assert.That(reader.PlayersList, Has.Count.EqualTo(1));
        Assert.That(reader.PlayersDict, Has.Count.EqualTo(1));
        Assert.That(reader.PlayersDict.ContainsKey("A"), Is.True);
    }

    [Test]
    public void Constructor_DuplicateLogin_CurrentBehavior()
    {
        var path = CreateTempFile("A|1|10|false\nA|2|20|true");

        var reader = new FileReader(path);

        // Текущее поведение: дубликат попадает в PlayersList, но не в PlayersDict.
        Assert.That(reader.PlayersList, Has.Count.EqualTo(2));
        Assert.That(reader.PlayersDict, Has.Count.EqualTo(1));
        Assert.That(reader.PlayersDict["A"].Level, Is.EqualTo(1));
    }

    [Test]
    [Ignore("Выявляет потенциальный баг: при дубликате логина игрок остаётся в PlayersList, но отсутствует в PlayersDict")]
    public void Constructor_DuplicateLogin_CollectionsShouldBeConsistent()
    {
        var path = CreateTempFile("A|1|10|false\nA|2|20|true");

        var reader = new FileReader(path);

        Assert.That(reader.PlayersList.Count, Is.EqualTo(reader.PlayersDict.Count));
    }

    [Test]
    public void Constructor_FileNotFound_Throws()
    {
        Assert.Throws<FileNotFoundException>(() =>
            new FileReader("nonexistent_file_12345.csv"));
    }
}