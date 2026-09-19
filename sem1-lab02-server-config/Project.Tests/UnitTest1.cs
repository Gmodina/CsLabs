
using Server.Config;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using NUnit.Framework;
using Server.Config;

namespace Server.Config.Tests;

[TestFixture]
public class ProgramTests
{
    private static readonly string[] Name1 =
    {
        "Fantastic", "Magic", "Unbelievable", "Rotten", "Dirty", "Fast", "Cubic"
    };

    private static readonly string[] Name2 =
    {
        "Craft", "MC", "World", "Place", "Room", "Bucket", "Surface"
    };

    [TestCase(1, false, 0, 0, Program.ServerState.Ready, "")]
    [TestCase(10, false, 199, 9, Program.ServerState.Ready, "")]
    [TestCase(32767, false, 0, 0, Program.ServerState.Ready, "")]

    [TestCase(1, true, 0, 0, Program.ServerState.Warning, "Сервер имеет пароль")]
    [TestCase(1, false, 0, 10, Program.ServerState.Warning, "Большое количество ошибок")]
    [TestCase(1, false, 0, 49, Program.ServerState.Warning, "Большое количество ошибок")]
    [TestCase(1, false, 200, 0, Program.ServerState.Warning, "Высокий пинг")]
    [TestCase(1, false, 499, 0, Program.ServerState.Warning, "Высокий пинг")]
    [TestCase(1, true, 200, 10, Program.ServerState.Warning, "Сервер имеет пароль|Большое количество ошибок|Высокий пинг")]
    [TestCase(1, false, 199, 10, Program.ServerState.Warning, "Большое количество ошибок")]
    [TestCase(1, false, 200, 9, Program.ServerState.Warning, "Высокий пинг")]
    [TestCase(1, true, 199, 9, Program.ServerState.Warning, "Сервер имеет пароль")]
    [TestCase(1, true, 499, 49, Program.ServerState.Warning, "Сервер имеет пароль|Большое количество ошибок|Высокий пинг")]
    [TestCase(1, false, 500, 10, Program.ServerState.Warning, "Большое количество ошибок")]
    [TestCase(1, true, 500, 0, Program.ServerState.Warning, "Сервер имеет пароль")]
    [TestCase(1, false, 200, 50, Program.ServerState.Warning, "Высокий пинг")]
    [TestCase(1, true, 0, 50, Program.ServerState.Warning, "Сервер имеет пароль")]

    [TestCase(0, false, 0, 0, Program.ServerState.Error, "Сервер не может быть запущен без игроков")]
    [TestCase(-1, true, 600, 100, Program.ServerState.Error, "Сервер не может быть запущен без игроков")]
    [TestCase(-32768, false, 0, 0, Program.ServerState.Error, "Сервер не может быть запущен без игроков")]

    [TestCase(1, false, 0, 51, Program.ServerState.Error, "Критическое количество ошибок!")]
    [TestCase(1, false, 0, 100, Program.ServerState.Error, "Критическое количество ошибок!")]
    [TestCase(1, false, 501, 0, Program.ServerState.Error, "Критический пинг!")]
    [TestCase(1, false, 1200, 0, Program.ServerState.Error, "Критический пинг!")]
    [TestCase(1, false, 501, 51, Program.ServerState.Error, "Критическое количество ошибок!|Критический пинг!")]

    [TestCase(1, false, 300, 60, Program.ServerState.Error, "Высокий пинг|Критическое количество ошибок!")]
    [TestCase(1, true, 300, 60, Program.ServerState.Error, "Сервер имеет пароль|Высокий пинг|Критическое количество ошибок!")]
    [TestCase(1, false, 600, 20, Program.ServerState.Error, "Большое количество ошибок|Критический пинг!")]
    [TestCase(1, true, 600, 20, Program.ServerState.Error, "Сервер имеет пароль|Большое количество ошибок|Критический пинг!")]
    [TestCase(1, true, 600, 60, Program.ServerState.Error, "Сервер имеет пароль|Критическое количество ошибок!|Критический пинг!")]

    [TestCase(0, true, 600, 60, Program.ServerState.Error, "Сервер не может быть запущен без игроков")]

    [TestCase(1, false, 0, 50, Program.ServerState.Error, "")]
    [TestCase(1, false, 500, 0, Program.ServerState.Error, "")]
    [TestCase(1, false, 500, 50, Program.ServerState.Error, "")]
    public void CheckConfiguration_ReturnsExpectedStateAndProblems(
        int playerCount,
        bool isPrivate,
        int ping,
        int errorsCount,
        Program.ServerState expectedState,
        string expectedProblemsCsv)
    {
        var actualState = Program.CheckConfiguration(
            (short)playerCount,
            isPrivate,
            (short)ping,
            errorsCount,
            out var problems);

        var expectedProblems = string.IsNullOrEmpty(expectedProblemsCsv)
            ? Array.Empty<string>()
            : expectedProblemsCsv.Split('|');

        Assert.That(problems, Is.Not.Null);
        Assert.That(actualState, Is.EqualTo(expectedState));
        Assert.That(problems, Is.EqualTo(expectedProblems));

        var server = new Program.Server
        {
            ServerName = "UnitTest",
            PlayerCount = (short)playerCount,
            IsPrivate = isPrivate,
            Ping = (short)ping,
            Ip = IPAddress.Loopback,
            Port = 25565,
            ErrorsCount = errorsCount,
            IsReady = Program.ServerState.Ready
        };

        var serverState = Program.CheckConfiguration(server, out var serverProblems);

        Assert.That(serverState, Is.EqualTo(expectedState));
        Assert.That(serverProblems, Is.EqualTo(expectedProblems));
    }

    [Test]
    public void CheckConfiguration_ServerOverload_IgnoresPresetIsReady()
    {
        var server = new Program.Server
        {
            ServerName = "IgnorePresetState",
            PlayerCount = 1,
            IsPrivate = false,
            Ping = 0,
            Ip = IPAddress.Loopback,
            Port = 25565,
            ErrorsCount = 0,
            IsReady = Program.ServerState.Error
        };

        var state = Program.CheckConfiguration(server, out var problems);

        Assert.That(state, Is.EqualTo(Program.ServerState.Ready));
        Assert.That(problems, Is.Empty);
    }

    [TestCase("")]
    [TestCase("abc")]
    [TestCase("Инициализация сервера")]
    [TestCase(" ")]
    public void ToCoolString_ReturnsWrappedString(string input)
    {
        var expected = $"------------------------------|{input}|------------------------------";
        Assert.That(Program.ToCoolString(input), Is.EqualTo(expected));
    }

    [Test]
    public void GenRandomName_ReturnsTwoWordsFromAllowedArrays()
    {
        for (var i = 0; i < 500; i++)
        {
            var name = InvokePrivateStatic<string>("GenRandomName");

            Assert.That(name, Is.Not.Null);
            Assert.That(name, Does.Contain(" "));

            var parts = name.Split(' ');
            Assert.That(parts.Length, Is.EqualTo(2));
            Assert.That(Name1, Does.Contain(parts[0]));
            Assert.That(Name2, Does.Contain(parts[1]));
        }
    }

    [Test]
    public void GenRandomIp_ReturnsIPv4Address()
    {
        for (var i = 0; i < 500; i++)
        {
            var ip = InvokePrivateStatic<IPAddress>("GenRandomIp");

            Assert.That(ip, Is.Not.Null);
            Assert.That(ip.AddressFamily, Is.EqualTo(System.Net.Sockets.AddressFamily.InterNetwork));
            Assert.That(ip.GetAddressBytes().Length, Is.EqualTo(4));
        }
    }

    [Test]
    public void GetServer_ReturnsServerWithinExpectedRanges()
    {
        for (var i = 0; i < 500; i++)
        {
            var server = InvokePrivateStatic<Program.Server>("GetServer");

            Assert.That(server.ServerName, Is.Not.Null);
            Assert.That(server.ServerName, Does.Contain(" "));
            Assert.That((int)server.PlayerCount, Is.InRange(0, short.MaxValue - 1));
            Assert.That((int)server.Ping, Is.InRange(0, 1199));
            Assert.That(server.Ip, Is.Not.Null);
            Assert.That(server.Ip.GetAddressBytes().Length, Is.EqualTo(4));
            Assert.That(server.Port, Is.InRange(2, 65534));
            Assert.That(server.ErrorsCount, Is.InRange(0, 99));
            Assert.That(server.IsReady, Is.EqualTo(Program.ServerState.Ready));
        }
    }

    [Test]
    [NonParallelizable]
    public void PrintServerStats_PrintsServerFieldsAndProblems()
    {
        var server = new Program.Server
        {
            ServerName = "Test Server",
            PlayerCount = 5,
            IsPrivate = true,
            Ping = 10,
            Ip = IPAddress.Loopback,
            Port = 25565,
            ErrorsCount = 0,
            IsReady = Program.ServerState.Warning
        };

        var output = CaptureConsoleOut(() => InvokePrivateStaticVoid("PrintServerStats", server));

        Assert.That(output, Does.Contain("Name: Test Server"));
        Assert.That(output, Does.Contain("PlCount: 5"));
        Assert.That(output, Does.Contain("Private: True"));
        Assert.That(output, Does.Contain("Ping: 10"));
        Assert.That(output, Does.Contain("IpAdress: 127.0.0.1:25565"));
        Assert.That(output, Does.Contain("Errors: 0"));
        Assert.That(output, Does.Contain("Status: Warning"));
        Assert.That(output, Does.Contain("Проблемы:"));
        Assert.That(output, Does.Contain("Сервер имеет пароль"));
    }

    [Test]
    [NonParallelizable]
    public void PrintServerStats_DoesNotPrintProblemsSection_WhenNoProblems()
    {
        var server = new Program.Server
        {
            ServerName = "Clean Server",
            PlayerCount = 1,
            IsPrivate = false,
            Ping = 0,
            Ip = IPAddress.Loopback,
            Port = 25565,
            ErrorsCount = 0,
            IsReady = Program.ServerState.Ready
        };

        var output = CaptureConsoleOut(() => InvokePrivateStaticVoid("PrintServerStats", server));

        Assert.That(output, Does.Contain("Name: Clean Server"));
        Assert.That(output, Does.Contain("Status: Ready"));
        Assert.That(output, Does.Not.Contain("Проблемы:"));
    }

    private static MethodInfo GetPrivateStaticMethod(string methodName)
    {
        var method = typeof(Program).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static);
        return method ?? throw new InvalidOperationException($"Метод {methodName} не найден.");
    }

    private static T InvokePrivateStatic<T>(string methodName)
    {
        var method = GetPrivateStaticMethod(methodName);
        var result = method.Invoke(null, null);
        return (T)result!;
    }

    private static void InvokePrivateStaticVoid(string methodName, params object[] args)
    {
        var method = GetPrivateStaticMethod(methodName);
        method.Invoke(null, args);
    }

    private static string CaptureConsoleOut(Action action)
    {
        var original = Console.Out;
        var writer = new StringWriter();

        Console.SetOut(writer);
        try
        {
            action();
        }
        finally
        {
            Console.SetOut(original);
        }

        return writer.ToString();
    }
}