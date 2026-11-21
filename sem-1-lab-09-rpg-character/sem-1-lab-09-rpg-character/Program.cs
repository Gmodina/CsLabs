namespace sem_1_lab_09_rpg_character;

public class Program
{
    public static void Main()
    {
        var pl = new Player("Seryoga", 1000.0f);
        
        pl.SpendGold(1000);
        pl.AddItem("Sword");
        pl.AddItem("Sword");
        pl.AddItem("Sword");
        pl.AddItem("Sword");
        pl.ShowStats();
    }
}

public class Player
{
    public string Name { get; }
    public CharacterClass Class { get; private set; } = 0;
    public uint Level { get; private set; } = 1;
    public uint Xp { get; private set; }
    public float Health { get; private set; }
    public float MaxHealth { get; }
    public float Gold { get; private set; } = 100.0f;
    public int Power { get; private set; } = 1;
    public int Defense { get; private set; } = 1;
    public float Mana { get; private set; } = 100.0f;
    public float MaxMana { get; private set; } = 100.0f;
    public List<string> Inventory { get; private set; } = [];

    public Player(string name, float maxHealth)
    {
        if (name is " " or "" or null) throw new ArgumentException("Value name can't be null");
        if (maxHealth < 0) throw new ArgumentException("Value maxHealth can't be negative");
        Name = name;
        MaxHealth = maxHealth;
        Health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage < 0) throw new ArgumentException("Value can't be negative");
        Health = Math.Clamp(Health - (damage - Defense), 0, MaxHealth);
    }

    public void RestoreHealth(float health)
    {
        if (health < 0) throw new ArgumentException("Value can't be negative");
        Health = Math.Clamp(health+Health, 0, MaxHealth);
    }

    public void AddXp(uint xp)
    {
        Xp += xp;
        uint levelCap = (uint)Math.Pow(4, Level) + 100;
        if (xp >= levelCap)
        {
            Xp -= levelCap;
            Level++;
        }
    }

    public void AddGold(int gold)
    {
        if (gold < 0) throw new ArgumentException("Value can't be negative");
        Gold += gold;
    }

    public void SpendGold(int gold)
    {
        if (gold < 0) throw new ArgumentException("Value can't be negative");
        Gold = Math.Max(Gold - gold, 0);
    }

    public void ChangePower(int power)
    {
        Power = Math.Max(Power + power, -100);
    }
    
    public void ChangeDefense(int defense)
    {
        Defense = Math.Max(Defense + defense, Convert.ToInt32(-MaxHealth));
    }

    public void ChangeClass(CharacterClass newClass)
    {
        Class = newClass;
    }
    
    public void SpendMana(float mana)
    {
        if (mana < 0) throw new ArgumentException("Value can't be negative");
        Mana = Math.Max(Mana - mana, 0);
    }

    public void RestoreMana(float mana)
    {
        if (mana < 0) throw new ArgumentException("Value can't be negative");
        Mana = Math.Min(mana+Mana, MaxMana);
    }

    public void ChangeMaxMana(float maxMana)
    {
        MaxMana = Math.Max(maxMana, 0);
    }

    public void AddItem(string item)
    {
        if (item is " " or "" or null) throw new InvalidOperationException("Value item can't be null");
        Inventory.Add(item);
    }

    public void RemoveItem(string item)
    {
        if (item is " " or "" or null) throw new InvalidOperationException("Value item can't be null");
        if (Inventory.Contains(item)) Inventory.Remove(item);
        else throw new InvalidOperationException($"Item {item} doesn't exist");
    }
    
    public void ShowStats()
    {
        Console.WriteLine($"Name: {Name}");
        Console.WriteLine($"Class: {Class}");
        Console.WriteLine($"Health: {Health}");
        Console.WriteLine($"MaxHealth: {MaxHealth}");
        Console.WriteLine($"Gold: {Gold}");
        Console.WriteLine($"Mana: {Mana}");
        Console.WriteLine($"Max Mana: {MaxMana}");
        Console.WriteLine($"Xp: {Xp}");
        Console.WriteLine($"Level: {Level}");
        Console.WriteLine($"Power: {Power}");
        Console.WriteLine($"Defense: {Defense}");
        Console.WriteLine($"Inventory: ");
        Inventory.ForEach(Console.WriteLine);
        
    }
}

public enum CharacterClass
{
    Warrior,
    Ranger,
    Mage,
    Summoner
}