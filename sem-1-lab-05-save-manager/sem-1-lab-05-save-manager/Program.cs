using Newtonsoft.Json;

namespace sem_1_lab_05_save_manager;

public class Program
{
    private static readonly string SavesDirectory = "saves";
    private static readonly string BackupDirectory = "backups";
    private static readonly string IndexPath = "index.json";
    private static readonly string LogPath = "manager_events.log";
    private static List<SaveEntry> _indexFile = LoadIndex(IndexPath);
    
    public static void Main()
    {
        while (true)
        {
            Console.Clear();
            PrintMenu(out var key);
            if (key == ConsoleKey.D7)
            {
                break;
            }
            HandlePressedKey(key, _indexFile, SavesDirectory, BackupDirectory, IndexPath);
            Console.Write("Нажмите любую клавишу для выхода в меню...");
            Console.ReadKey();
        }
    }
    
    public class SaveData
    {
        public string Character { get; set; }
        public string Map { get; set; }
        public Position Position { get; set; }
        public int Health { get; set; }
        public List<string> Inventory { get; set; }
    }
    
    public struct Position
    {
        public int X;
        public int Y;
    }
    
    public class SaveEntry
    {
        public string Name { get; set; }
        public string File { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public static List<SaveEntry> LoadIndex(string indexPath)
    {
        List<SaveEntry> indexData = [];
        
        try
        {
            string file = File.ReadAllText(indexPath);
            indexData = JsonConvert.DeserializeObject<List<SaveEntry>>(file);
        }
        catch
        {
            Console.WriteLine("Не удалось получить файл");
        }

        return indexData;
    }

    public static SaveData LoadSave(string savesDirectory, SaveEntry entry)
    {
        SaveData save = new SaveData();

        try
        {
            string pathToSave = $"{savesDirectory}\\{entry.File}";
            string file = File.ReadAllText(pathToSave);
            save = JsonConvert.DeserializeObject<SaveData>(file);
            return save;
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Указанный файл не найден");
        }
        catch (JsonReaderException)
        {
            Console.WriteLine("Не удалось прочитать файл");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return save;
    }

    public static void PrintSave(SaveEntry entry, SaveData save)
    {
        string name = entry.Name;
        string character = save.Character;
        string map = save.Map;
        string position = $"Y:{save.Position.Y.ToString()} X:{save.Position.X.ToString()}";
        string health = save.Health.ToString();
        string[] inventory = save.Inventory.ToArray();
        
        Console.WriteLine($"-|Сохранение {name}|-");
        Console.WriteLine($"Персонаж: {character}");
        Console.WriteLine($"Карта: {map}");
        Console.WriteLine($"Координаты: {position}");
        Console.WriteLine($"Здоровье: {health}");
        Console.WriteLine("Инвентарь:");
        foreach (string item in inventory)
        { 
            Console.WriteLine(item);
        }
    }

    public static SaveEntry FindByName(List<SaveEntry> saveEntries, string name)
    {
        SaveEntry save = new SaveEntry();
        bool flag = true;
        
        foreach (var entry in saveEntries)
        {
            if (entry.Name.ToLowerInvariant() == name.ToLowerInvariant())
            {
                save = entry;
                flag = false;
            }
        }
        if (flag) throw new FileNotFoundException();
        return save;
    }

    public static void SaveGame(string savesDirectory, SaveEntry entry, SaveData save)
    {
        string path = $"{savesDirectory}\\{entry.File}";
        string file = JsonConvert.SerializeObject(save, Formatting.Indented);
        
        File.WriteAllText(path, file);
    }

    public static void SaveIndex(string indexPath, List<SaveEntry> entries)
    {
        string file = JsonConvert.SerializeObject(entries,  Formatting.Indented);
        
        File.WriteAllText(indexPath, file);
    }

    public static void CreateBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string pathToBackup = $"{backupsDirectory}\\{entry.File}";
        string pathToSave = $"{savesDirectory}\\{entry.File}";
        
        if (File.Exists(pathToSave))
        {
            File.Copy(pathToSave, pathToBackup, true);
            Console.WriteLine($"Бекап {entry.Name} успешно создан");
        }
        else
        {
            Console.WriteLine($"Не удалось найти файл по пути {pathToSave}");
            throw new ArgumentException("Can't access file");
        }
    }

    public static void RestoreBackup(string savesDirectory, string backupsDirectory, SaveEntry entry)
    {
        string pathToBackup = $"{backupsDirectory}\\{entry.File}";
        string pathToSave = $"{savesDirectory}\\{entry.File}";
        
        if (File.Exists(pathToBackup))
        {
            File.Copy(pathToBackup,pathToSave, true);
            Console.WriteLine($"Бекап {entry.Name} успешно восстановлен");
        }
        else
        {
            Console.WriteLine($"Не удалось найти файл по пути {pathToBackup}");
        }
    }

    public static void DeleteSave(string savesDirectory, string indexPath, List<SaveEntry> entries, SaveEntry entry)
    {
        string pathToSave = $"{savesDirectory}\\{entry.File}";
        
        if (File.Exists(pathToSave))
        {
            File.Delete(pathToSave);
            Console.WriteLine($"Файл сохранения {entry.Name} удалён");
        }
        else
        {
            Console.WriteLine($"Файл сохранения {entry.Name} не найден");
        }
        
        entries.Remove(entry);
        string newEntries = JsonConvert.SerializeObject(entries, Formatting.Indented);
        File.WriteAllText(indexPath, newEntries);
    }

    public static string ToCoolString(string str)
    {
        return $"================================|{str}|================================";
    }

    private static void PrintMenu(out ConsoleKey pressedKey)
    {
        Console.WriteLine("========================Меню========================");
        Console.WriteLine("1. Показать список сохранений.");
        Console.WriteLine("2. Посмотреть данные сохранения.");
        Console.WriteLine("3. Изменить данные персонажа и записать их обратно.");
        Console.WriteLine("4. Создать резервную копию.");
        Console.WriteLine("5. Восстановить резервную копию.");
        Console.WriteLine("6. Удалить сохранение.");
        Console.WriteLine("7. Выйти.");
        Console.WriteLine("====================================================");
        
        pressedKey = Console.ReadKey().Key;
    }

    private static string ReadSaveName()
    {
        Console.Clear();
        Console.WriteLine(ToCoolString("Выбор сохранения"));
        Console.Write("\nВведите имя сохранения: ");
        string saveName;
        while (true)
        {
            saveName = Console.ReadLine();
            if (saveName!=null) break;
        }
        return saveName;
    }
    
    private static void AddNewLogString(string saveName, string act, bool isSuccessfully)
    {
        DateTime now = DateTime.Now.ToLocalTime();
        string message =  $"\n[{now}] \"{act}\" сохранение: {saveName}. Удачно: {isSuccessfully}";
        File.AppendAllText(LogPath, message);
    }

    private static void HandlePressedKey(ConsoleKey key, List<SaveEntry> entries, string savesDirectory, string backupsDirectory, string indexPath)
    {
        switch (key)
        {
            case ConsoleKey.D1:
            {
                Console.Clear();
                Console.WriteLine(ToCoolString("Сохранения"));
                foreach (var entry in entries)
                {
                    Console.WriteLine(entry.Name);
                }

                File.AppendAllText(LogPath, $"\n[{DateTime.Now.ToLocalTime()}] \"Просмотрены все сохранения\". Удачно: true");
                break;
            }
            
            case ConsoleKey.D2:
            {
                Console.Clear();
                var saveName = ReadSaveName();
                bool flag = true;
                Console.Clear();
                Console.WriteLine(ToCoolString("Просмотреть данные сохранения"));
                
                foreach (var entry in entries)
                {
                    if (entry.Name == saveName)
                    {
                        PrintSave(entry, LoadSave(savesDirectory, entry));
                        flag = false;
                        AddNewLogString(saveName, "Просмотр данных сохранения", true);
                        break;
                    }
                }
                if (flag)
                {
                    Console.WriteLine($"Файл {saveName} не найден");
                    AddNewLogString(saveName, "Просмотр данных сохранения", false);
                }
                break;
            }
            
            case ConsoleKey.D3:
            {
                string saveName = ReadSaveName();
                SaveEntry saveEntry =  new SaveEntry();
                try
                {
                    saveEntry = FindByName(entries, saveName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Не удалось прочитать сохранение");
                    Console.WriteLine(e.Message);
                    AddNewLogString(saveName, "Изменение хар-ик персонажа", false);
                    break;
                }
                SaveData saveData = LoadSave(savesDirectory, saveEntry);
                Console.Clear();
                Console.WriteLine(ToCoolString("Изменение хар-ик персонажа"));
                Console.WriteLine("Что вы хотите поменять?");
                Console.WriteLine("1. Карту\n" +
                                  "2. Координаты\n" +
                                  "3. Здоровье\n" +
                                  "4. Предметы инвентаря\n");
                ConsoleKey pressedKey = Console.ReadKey().Key;
                Console.Clear();
                if (pressedKey == ConsoleKey.D1)
                {
                    Console.Write("Введите значение, на которое хотите поменять карту: ");
                    string newMap = saveData.Map;
                    try
                    {
                        newMap = Console.ReadLine();
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", true);
                    }
                    catch
                    {
                        Console.WriteLine("Не указано значений, оставляю без изменений");
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", false);
                    }
                    saveData.Map = newMap;
                }
                else if (pressedKey == ConsoleKey.D2)
                {
                    Position newPos = saveData.Position;
                    Console.Write("Введите значения, на которые хотите поменять позицию(X Y): ");
                    try
                    {
                        string[] buff = Console.ReadLine().Split(' ');
                        newPos.X = Convert.ToInt32(buff[0]);
                        newPos.Y = Convert.ToInt32(buff[1]);
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", true);
                    }
                    catch
                    {
                        Console.WriteLine("Не указано значений, либо они указаны неправильно. Оставляю без изменений");
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", false);
                    }
                    saveData.Position = newPos;
                }
                else if (pressedKey == ConsoleKey.D3)
                {
                    Console.Write("Введите значение, на которое хотите поменять здоровье: ");
                    int newHealth = saveData.Health;
                    try
                    {
                        newHealth = Convert.ToInt32(Console.ReadLine());
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", true);
                    }
                    catch
                    {
                        Console.WriteLine("Не указано значений, либо они указаны неправильно. Оставляю без изменений");
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", false);
                    }
                    saveData.Health = newHealth;
                }
                else
                {
                    List<string> newItems = saveData.Inventory;
                    Console.Write("Введите значения, на которые хотите поменять предметы(I1,I2,I3,...): ");
                    try
                    {
                        string[] buff = Console.ReadLine().Split(',');
                        newItems.Clear();
                        foreach (var item in buff)
                        {
                            newItems.Add(item);
                        }
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", true);
                    }
                    catch
                    {
                        Console.WriteLine("Не указано значений, либо они указаны неправильно. Оставляю без изменений");
                        AddNewLogString(saveName, "Изменение хар-ик персонажа", false);
                    }
                    saveData.Inventory = newItems;
                    saveEntry.UpdatedAt = DateTime.Now.ToLocalTime();
                }
                SaveGame(savesDirectory,saveEntry, saveData);
                SaveIndex(indexPath, entries);
                break;
            }
            
            case ConsoleKey.D4:
            {
                string saveName = ReadSaveName();
                try
                {
                    CreateBackup(savesDirectory, backupsDirectory, FindByName(entries, saveName));
                    AddNewLogString(saveName, "Создание бэкапа", true);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                    AddNewLogString(saveName, "Создание бэкапа", false);
                }
                
                break;
            }
            
            case ConsoleKey.D5:
            {
                string saveName = ReadSaveName();
                try
                {
                    RestoreBackup(savesDirectory, backupsDirectory, FindByName(entries, saveName));
                    AddNewLogString(saveName, "Восстановление бэкапа", true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    AddNewLogString(saveName, "Восстановление бэкапа", false);
                }
                break;
            }
            
            case ConsoleKey.D6:
            {
                string saveName = ReadSaveName();

                try
                {
                    DeleteSave(savesDirectory, indexPath, entries, FindByName(entries, saveName));
                    AddNewLogString(saveName, "Удаление сохранения", true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    AddNewLogString(saveName, "Удаление сохранения", false);
                }
                break;
            }
        }
    }
}