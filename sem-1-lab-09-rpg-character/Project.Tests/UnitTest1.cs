using sem_1_lab_09_rpg_character;

namespace Project.Tests;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Constructor_ValidArguments_SetsProperties()
    {
        var player = new Player("Seryoga", 1000f);

        Assert.That(player.Name, Is.EqualTo("Seryoga"));
        Assert.That(player.MaxHealth, Is.EqualTo(1000f));
        Assert.That(player.Health, Is.EqualTo(1000f)); 
        Assert.That(player.Gold, Is.EqualTo(100f));
        Assert.That(player.Power, Is.EqualTo(1));
        Assert.That(player.Defense, Is.EqualTo(1));
        Assert.That(player.Mana, Is.EqualTo(100f));
        Assert.That(player.MaxMana, Is.EqualTo(100f));
        Assert.That(player.Level, Is.EqualTo(1u));
        Assert.That(player.Xp, Is.EqualTo(0u));
        Assert.That(player.Class, Is.EqualTo(CharacterClass.Warrior));
        Assert.That(player.Inventory, Is.Empty);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Constructor_InvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() => new Player(name!, 100f));
    }

    [Test]
    public void Constructor_NegativeMaxHealth_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Player("Test", -1f));
    }

    [Test]
    public void TakeDamage_ReducesHealthByDamageMinusDefense()
    {
        var player = new Player("Test", 1000f);

        player.TakeDamage(100f);

        // 1000 - (100 - 1) = 901
        Assert.That(player.Health, Is.EqualTo(901f));
    }

    [Test]
    public void TakeDamage_ClampsHealthToZero()
    {
        var player = new Player("Test", 1000f);

        player.TakeDamage(2000f);

        Assert.That(player.Health, Is.EqualTo(0f));
    }

    [Test]
    public void TakeDamage_NegativeDamage_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.TakeDamage(-1f));
    }

    [Test]
    public void RestoreHealth_IncreasesHealth()
    {
        var player = new Player("Test", 1000f);
        player.TakeDamage(500f); // 1000 - 499 = 501

        player.RestoreHealth(250f); // 501 + 250 = 751

        Assert.That(player.Health, Is.EqualTo(751f));
    }

    [Test]
    public void RestoreHealth_ClampsToMaxHealth()
    {
        var player = new Player("Test", 1000f);
        player.TakeDamage(500f); // Health = 501

        player.RestoreHealth(2000f);

        Assert.That(player.Health, Is.EqualTo(1000f));
    }

    [Test]
    public void RestoreHealth_NegativeHealth_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.RestoreHealth(-1f));
    }

    [Test]
    public void AddXp_IncreasesXpWithoutLevelUp()
    {
        var player = new Player("Test", 100f);

        player.AddXp(50);

        Assert.That(player.Xp, Is.EqualTo(50u));
        Assert.That(player.Level, Is.EqualTo(1u));
    }

    [Test]
    public void AddXp_EnoughForLevelUp_IncreasesLevelAndResetsXp()
    {
        var player = new Player("Test", 100f);
        // levelCap для Level=1: 4^1 + 100 = 104

        player.AddXp(104);

        Assert.That(player.Level, Is.EqualTo(2u));
        Assert.That(player.Xp, Is.EqualTo(0u));
    }

    [Test]
    public void AddXp_MoreThanLevelCap_LevelsUpOnlyOnce()
    {
        var player = new Player("Test", 100f);

        player.AddXp(500);

        Assert.That(player.Level, Is.EqualTo(2u));
        Assert.That(player.Xp, Is.EqualTo(396u)); // 500 - 104
    }

    [Test]
    public void AddGold_IncreasesGold()
    {
        var player = new Player("Test", 100f);

        player.AddGold(50);

        Assert.That(player.Gold, Is.EqualTo(150f));
    }

    [Test]
    public void AddGold_NegativeGold_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.AddGold(-1));
    }

    [Test]
    public void SpendGold_ReducesGold()
    {
        var player = new Player("Test", 100f);

        player.SpendGold(40);

        Assert.That(player.Gold, Is.EqualTo(60f));
    }

    [Test]
    public void SpendGold_ClampsToZero()
    {
        var player = new Player("Test", 100f);

        player.SpendGold(1000);

        Assert.That(player.Gold, Is.EqualTo(0f));
    }

    [Test]
    public void SpendGold_NegativeGold_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.SpendGold(-1));
    }

    [Test]
    public void ChangePower_IncreasesPower()
    {
        var player = new Player("Test", 100f);

        player.ChangePower(10);

        Assert.That(player.Power, Is.EqualTo(11));
    }

    [Test]
    public void ChangePower_ClampsToMinus100()
    {
        var player = new Player("Test", 100f);

        player.ChangePower(-200);

        Assert.That(player.Power, Is.EqualTo(-100));
    }

    [Test]
    public void ChangeDefense_IncreasesDefense()
    {
        var player = new Player("Test", 100f);

        player.ChangeDefense(5);

        Assert.That(player.Defense, Is.EqualTo(6));
    }

    [Test]
    public void ChangeDefense_ClampsToMinusMaxHealth()
    {
        var player = new Player("Test", 100f);

        player.ChangeDefense(-200);

        Assert.That(player.Defense, Is.EqualTo(-100)); // max(-199, -100)
    }

    [Test]
    public void ChangeClass_SetsNewClass()
    {
        var player = new Player("Test", 100f);

        player.ChangeClass(CharacterClass.Mage);

        Assert.That(player.Class, Is.EqualTo(CharacterClass.Mage));
    }

    [Test]
    public void SpendMana_ReducesMana()
    {
        var player = new Player("Test", 100f);

        player.SpendMana(30f);

        Assert.That(player.Mana, Is.EqualTo(70f));
    }

    [Test]
    public void SpendMana_ClampsToZero()
    {
        var player = new Player("Test", 100f);

        player.SpendMana(1000f);

        Assert.That(player.Mana, Is.EqualTo(0f));
    }

    [Test]
    public void SpendMana_NegativeMana_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.SpendMana(-1f));
    }

    [Test]
    public void RestoreMana_IncreasesMana()
    {
        var player = new Player("Test", 100f);
        player.SpendMana(50f); // Mana = 50

        player.RestoreMana(30f);

        Assert.That(player.Mana, Is.EqualTo(80f));
    }

    [Test]
    public void RestoreMana_ClampsToMaxMana()
    {
        var player = new Player("Test", 100f);
        player.SpendMana(50f); // Mana = 50

        player.RestoreMana(1000f);

        Assert.That(player.Mana, Is.EqualTo(100f));
    }

    [Test]
    public void RestoreMana_NegativeMana_ThrowsArgumentException()
    {
        var player = new Player("Test", 100f);
        Assert.Throws<ArgumentException>(() => player.RestoreMana(-1f));
    }

    [Test]
    public void ChangeMaxMana_SetsMaxMana()
    {
        var player = new Player("Test", 100f);

        player.ChangeMaxMana(50f);

        Assert.That(player.MaxMana, Is.EqualTo(50f));
    }

    [Test]
    public void ChangeMaxMana_NegativeValue_ClampsToZero()
    {
        var player = new Player("Test", 100f);

        player.ChangeMaxMana(-10f);

        Assert.That(player.MaxMana, Is.EqualTo(0f));
    }

    [Test]
    public void AddItem_AddsItemToInventory()
    {
        var player = new Player("Test", 100f);

        player.AddItem("Sword");

        Assert.That(player.Inventory, Contains.Item("Sword"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void AddItem_InvalidItem_ThrowsInvalidOperationException(string? item)
    {
        var player = new Player("Test", 100f);
        Assert.Throws<InvalidOperationException>(() => player.AddItem(item!));
    }

    [Test]
    public void RemoveItem_RemovesExistingItem()
    {
        var player = new Player("Test", 100f);
        player.AddItem("Sword");

        player.RemoveItem("Sword");

        Assert.That(player.Inventory, Is.Empty);
    }

    [Test]
    public void RemoveItem_NotExistingItem_ThrowsInvalidOperationException()
    {
        var player = new Player("Test", 100f);

        Assert.Throws<InvalidOperationException>(() => player.RemoveItem("Axe"));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void RemoveItem_InvalidItem_ThrowsInvalidOperationException(string? item)
    {
        var player = new Player("Test", 100f);
        Assert.Throws<InvalidOperationException>(() => player.RemoveItem(item!));
    }

    [Test]
    public void ShowStats_PrintsAllStats()
    {
        var player = new Player("Seryoga", 1000f);
        player.AddItem("Sword");

        var originalOut = Console.Out;
        using var sw = new StringWriter();
        Console.SetOut(sw);
        try
        {
            player.ShowStats();
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = sw.ToString();
        Assert.That(output, Does.Contain("Name: Seryoga"));
        Assert.That(output, Does.Contain("Class: Warrior"));
        Assert.That(output, Does.Contain("Health: 1000")); 
        Assert.That(output, Does.Contain("MaxHealth: 1000"));
        Assert.That(output, Does.Contain("Gold: 100"));
        Assert.That(output, Does.Contain("Mana: 100"));
        Assert.That(output, Does.Contain("Max Mana: 100"));
        Assert.That(output, Does.Contain("Xp: 0"));
        Assert.That(output, Does.Contain("Level: 1"));
        Assert.That(output, Does.Contain("Power: 1"));
        Assert.That(output, Does.Contain("Defense: 1"));
        Assert.That(output, Does.Contain("Inventory:"));
        Assert.That(output, Does.Contain("Sword"));
    }
}