using NUnit.Framework;
using sem_1_lab_06_realtime_monitor;
using System.Linq;

namespace sem_1_lab_06_realtime_monitor.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        [SetUp]
        public void Setup()
        {
            // Сбрасываем статические поля перед каждым тестом, 
            // чтобы тесты не зависели друг от друга
            Program._cpuUsage = new double[20];
            Program._ramUsage = new double[20];
            Program._temperature = new double[20];
            Program._currIndex = 0;
        }

        [Test]
        public void BuildGraph_MixedValues_ReturnsCorrectBlocks()
        {
            // Arrange: значения для всех диапазонов (>=75, 50-75, 25-50, 0-25)
            double[] arr = { 80, 60, 30, 10 };

            // Act
            string result = Program.BuildGraph(arr);

            // Assert
            Assert.That(result, Is.EqualTo("█ ▓ ▒ ░ "));
        }

        [Test]
        public void BuildGraph_AllHighValues_ReturnsFullBlocks()
        {
            // Arrange: массив из 20 высоких значений
            double[] arr = Enumerable.Repeat(80.0, 20).ToArray();

            // Act
            string result = Program.BuildGraph(arr);

            // Assert
            string expected = string.Join("", Enumerable.Repeat("█ ", 20));
            Assert.That(result, Is.EqualTo(expected));
        }

        [Test]
        public void OnDataReceived_UpdatesStateCorrectly()
        {
            // Act: передаем первые данные
            Program.OnDataReceived(10, 20, 30);

            // Assert: проверяем, что данные записались и индекс сдвинулся
            Assert.That(Program._cpuUsage[0], Is.EqualTo(10));
            Assert.That(Program._ramUsage[0], Is.EqualTo(20));
            Assert.That(Program._temperature[0], Is.EqualTo(30));
            Assert.That(Program._currIndex, Is.EqualTo(1));
        }

        [Test]
        public void OnDataReceived_ShiftsArrayWhenFull()
        {
            // Arrange & Act: Заполняем массив полностью (размер 20, индексы 0..19)
            for (int i = 0; i < 20; i++)
            {
                Program.OnDataReceived(i, i, i);
            }

            // Проверяем состояние перед последним сдвигом
            Assert.That(Program._currIndex, Is.EqualTo(19));
            Assert.That(Program._cpuUsage[0], Is.EqualTo(1)); // После первого сдвига (на шаге i=19)
            Assert.That(Program._cpuUsage[19], Is.EqualTo(19));

            // Act: Добавляем еще одно значение, должен сработать алгоритм сдвига
            Program.OnDataReceived(100, 100, 100);

            // Assert: Проверяем, что массив корректно сдвинулся 
            Assert.That(Program._cpuUsage[0], Is.EqualTo(2));
            Assert.That(Program._cpuUsage[18], Is.EqualTo(19));
            Assert.That(Program._cpuUsage[19], Is.EqualTo(100)); // Новое значение встало в конец
            Assert.That(Program._currIndex, Is.EqualTo(19)); // Индекс не вышел за пределы
        }
    }
}