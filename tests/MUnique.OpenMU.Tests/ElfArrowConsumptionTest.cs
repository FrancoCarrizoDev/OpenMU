// <copyright file="ElfArrowConsumptionTest.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Moq;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MonsterDefinition = MUnique.OpenMU.Persistence.BasicModel.MonsterDefinition;
using MonsterAttribute = MUnique.OpenMU.Persistence.BasicModel.MonsterAttribute;

/// <summary>
/// Regression tests for the elf-arrows-no-consume behavior: fairy elf / muse elf characters must
/// not consume bow or crossbow ammunition when attacking, while other classes keep the original
/// behavior. See <c>ClassFairyElf.CreateFairyElf</c> for the implementation.
/// </summary>
[TestFixture]
public class ElfArrowConsumptionTest
{
    private const byte BowsGroup = 4;
    private const byte ShortBowNumber = 0;
    private const byte ArrowsNumber = 15;
    private const byte BoltNumber = 7;
    private const byte CrossbowNumber = 8;

    private IGameContext _gameContext = null!;

    /// <summary>
    /// Sets up a fresh game context before each test.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        this._gameContext = GameContextTestHelper.CreateGameContext();
    }

    /// <summary>
    /// An elf character (fairy elf / muse elf) with a bow and arrows must not consume arrows
    /// after any number of attacks: the elf class zeroes the ammunition consumption rate via
    /// an attribute relationship, so <see cref="AttackableExtensions.ApplyAmmunitionConsumption"/>
    /// is a no-op even after the bow contributes its +1 rate.
    /// </summary>
    [Test]
    public async ValueTask FairyElf_DoesNotConsumeArrows_WhenAttackingAsync()
    {
        var context = await this.CreatePlayerWithBowAndArrowsAsync(isElf: true, useCrossbow: false).ConfigureAwait(false);
        var player = context.Player;
        var arrows = context.AmmoItem;
        const int hitCount = 25;
        var monster = await this.CreateAdjacentMonsterAsync(player).ConfigureAwait(false);
        await player.CurrentMap!.AddAsync(monster).ConfigureAwait(false);

        Assert.That(player.Attributes![Stats.AmmunitionConsumptionRate], Is.EqualTo(0f),
            "Elf class must zero the ammunition consumption rate while a bow is equipped.");
        Assert.That(player.Attributes[Stats.AmmunitionAmount], Is.EqualTo(context.InitialAmmoDurability),
            "AmmunitionAmount must mirror the equipped arrows' durability on (re)equip.");

        for (var i = 0; i < hitCount; i++)
        {
            await monster.AttackByAsync(player, null, false).ConfigureAwait(false);
        }

        Assert.Multiple(() =>
        {
            Assert.That(arrows.Durability, Is.EqualTo(context.InitialAmmoDurability),
                "Fairy elf arrows must not lose durability on attack.");
            Assert.That(player.Attributes[Stats.AmmunitionAmount], Is.EqualTo(context.InitialAmmoDurability),
                "AmmunitionAmount must stay in sync with the arrows' durability.");
        });
    }

    /// <summary>
    /// Same expectation for bolts when the elf is wielding a crossbow instead of a bow.
    /// </summary>
    [Test]
    public async ValueTask MuseElf_DoesNotConsumeBolts_WhenAttackingWithCrossbowAsync()
    {
        var context = await this.CreatePlayerWithBowAndArrowsAsync(isElf: true, useCrossbow: true).ConfigureAwait(false);
        var player = context.Player;
        var bolts = context.AmmoItem;
        const int hitCount = 25;
        var monster = await this.CreateAdjacentMonsterAsync(player).ConfigureAwait(false);
        await player.CurrentMap!.AddAsync(monster).ConfigureAwait(false);

        Assert.That(player.Attributes![Stats.AmmunitionConsumptionRate], Is.EqualTo(0f),
            "Elf class must zero the ammunition consumption rate while a crossbow is equipped.");

        for (var i = 0; i < hitCount; i++)
        {
            await monster.AttackByAsync(player, null, false).ConfigureAwait(false);
        }

        Assert.That(bolts.Durability, Is.EqualTo(context.InitialAmmoDurability),
            "Muse elf bolts must not lose durability on attack with a crossbow.");
    }

    /// <summary>
    /// Non-elf characters (here a dark-knight-like class with no elf rule) must keep the original
    /// behavior: arrows are consumed on every attack with a bow.
    /// </summary>
    [Test]
    public async ValueTask NonElf_StillConsumesArrows_WhenAttackingAsync()
    {
        var context = await this.CreatePlayerWithBowAndArrowsAsync(isElf: false, useCrossbow: false).ConfigureAwait(false);
        var player = context.Player;
        var arrows = context.AmmoItem;
        var monster = await this.CreateAdjacentMonsterAsync(player).ConfigureAwait(false);
        await player.CurrentMap!.AddAsync(monster).ConfigureAwait(false);

        Assert.That(player.Attributes![Stats.AmmunitionConsumptionRate], Is.EqualTo(1f),
            "Non-elf classes must keep the bow's +1 consumption rate.");

        var hits = 5;
        for (var i = 0; i < hits; i++)
        {
            await monster.AttackByAsync(player, null, false).ConfigureAwait(false);
        }

        Assert.That(arrows.Durability, Is.EqualTo(context.InitialAmmoDurability - hits),
            "Non-elf arrows must lose durability on attack, one per hit.");
    }

    /// <summary>
    /// Sanity check: the elf rule only kicks in when a bow/crossbow is equipped. Without one, an
    /// elf's <see cref="Stats.AmmunitionConsumptionRate"/> is simply the default (0).
    /// </summary>
    [Test]
    public async ValueTask FairyElf_WithoutRangedWeapon_HasNoConsumptionRateAsync()
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);
        this.ReplaceCharacterClassWith(player, BuildElfCharacterClassMock().Object);

        Assert.That(player.Attributes![Stats.AmmunitionConsumptionRate], Is.EqualTo(0f));
        Assert.That(player.Attributes[Stats.ArcheryAttackMode], Is.EqualTo(0f));
    }

    private async ValueTask<(Player Player, Item AmmoItem, int InitialAmmoDurability)> CreatePlayerWithBowAndArrowsAsync(bool isElf, bool useCrossbow)
    {
        var player = await PlayerTestHelper.CreatePlayerAsync(this._gameContext).ConfigureAwait(false);

        var weaponNumber = useCrossbow ? CrossbowNumber : ShortBowNumber;
        var ammoNumber = useCrossbow ? BoltNumber : ArrowsNumber;
        var weaponSlot = useCrossbow ? InventoryConstants.LeftHandSlot : InventoryConstants.RightHandSlot;
        var ammoSlot = useCrossbow ? InventoryConstants.RightHandSlot : InventoryConstants.LeftHandSlot;
        var isBowEquippedFlag = useCrossbow ? Stats.IsCrossBowEquipped : Stats.IsBowEquipped;

        var weaponDefinition = this.CreateWeaponDefinition(player, weaponNumber, weaponSlot, isBowEquippedFlag);
        var ammoDefinition = this.CreateAmmoDefinition(player, ammoNumber, ammoSlot);

        this.ReplaceCharacterClassWith(
            player,
            (isElf ? BuildElfCharacterClassMock() : BuildNonElfCharacterClassMock()).Object);

        const int initialDurability = 100;
        var weapon = CreateItem(weaponDefinition, initialDurability);
        var ammo = CreateItem(ammoDefinition, initialDurability);

        await player.Inventory!.AddItemAsync(weaponSlot, weapon).ConfigureAwait(false);
        await player.Inventory.AddItemAsync(ammoSlot, ammo).ConfigureAwait(false);

        Assert.That(player.Inventory.EquippedAmmunitionItem, Is.SameAs(ammo),
            "Sanity: the ammo item must be discoverable through InventoryStorage.EquippedAmmunitionItem.");

        return (player, ammo, initialDurability);
    }

    private void ReplaceCharacterClassWith(Player player, CharacterClass characterClass)
    {
        var character = player.SelectedCharacter!;
        character.CharacterClass = characterClass;

        // PlayerTestHelper.CreatePlayerAsync pre-populates SelectedCharacter.Attributes from the
        // *original* (generic) character class. Clear and re-populate with the new class's stats
        // so the AttributeSystem constructor doesn't see duplicate keys.
        character.Attributes.Clear();
        foreach (var statAttribute in characterClass.StatAttributes)
        {
            character.Attributes.Add(new StatAttribute(statAttribute.Attribute!, statAttribute.BaseValue));
        }

        // Player.Attributes has a private setter, so we use reflection to rebind the attribute
        // system after replacing the character class. The default PlayerTestHelper player is
        // created with a generic class; for these tests we need one wired to the elf/non-elf
        // AttributeCombinations.
        var newAttributes = new ItemAwareAttributeSystem(
            player.Account!,
            character,
            this._gameContext.Configuration);
        var attributesProperty = typeof(Player).GetProperty(nameof(Player.Attributes))!;
        attributesProperty.SetValue(player, newAttributes);

        // Player.SetSelectedCharacterAsync normally attaches OnAmmunitionAmountChanged to the
        // AmmunitionAmount attribute so that durability writes back to the equipped item. Since
        // we replaced the attribute system after the fact, we need to re-attach that handler
        // here, otherwise ApplyAmmunitionConsumption would decrement the attribute without
        // updating the arrow's durability on the mocked Item.
        var ammoAttribute = newAttributes.GetOrCreateAttribute(Stats.AmmunitionAmount);
        var handlerMethod = typeof(Player).GetMethod(
            "OnAmmunitionAmountChanged",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?? throw new InvalidOperationException("OnAmmunitionAmountChanged not found on Player.");
        var handlerDelegate = (EventHandler)Delegate.CreateDelegate(typeof(EventHandler), player, handlerMethod);
        ammoAttribute.ValueChanged += handlerDelegate;
    }

    private static Mock<CharacterClass> BuildElfCharacterClassMock()
    {
        var mock = new Mock<CharacterClass>();
        mock.SetupAllProperties();
        mock.Setup(c => c.StatAttributes).Returns(new List<StatAttributeDefinition>
        {
            new (Stats.Level, 1, false),
            new (Stats.BaseStrength, 22, true),
            new (Stats.BaseAgility, 25, true),
            new (Stats.BaseVitality, 20, true),
            new (Stats.BaseEnergy, 15, true),
            new (Stats.CurrentHealth, 100, false),
            new (Stats.CurrentMana, 100, false),
            new (Stats.CurrentAbility, 0, false),
            new (Stats.AmmunitionAmount, 0, false),
            new (Stats.IsInSafezone, 1, false),
            new (Stats.Resets, 0, false),
        });
        mock.Setup(c => c.AttributeCombinations).Returns(new List<AttributeRelationship>
        {
            new (Stats.ArcheryAttackMode, 1, Stats.IsBowEquipped),
            new (Stats.ArcheryAttackMode, 1, Stats.IsCrossBowEquipped),
            new AttributeRelationship(Stats.AmmunitionConsumptionRate, 0f, Stats.AmmunitionAmount, InputOperator.Multiply, null, AggregateType.Multiplicate),
        });
        mock.Setup(c => c.BaseAttributeValues).Returns(new List<ConstValueAttribute>
        {
            new (1, Stats.SkillMultiplier),
            new (1, Stats.AttackDamageIncrease),
            new (1, Stats.DamageReceiveDecrement),
        });
        return mock;
    }

    private static Mock<CharacterClass> BuildNonElfCharacterClassMock()
    {
        var mock = new Mock<CharacterClass>();
        mock.SetupAllProperties();
        mock.Setup(c => c.StatAttributes).Returns(new List<StatAttributeDefinition>
        {
            new (Stats.Level, 1, false),
            new (Stats.BaseStrength, 28, true),
            new (Stats.BaseAgility, 20, true),
            new (Stats.BaseVitality, 25, true),
            new (Stats.BaseEnergy, 10, true),
            new (Stats.CurrentHealth, 100, false),
            new (Stats.CurrentMana, 0, false),
            new (Stats.CurrentAbility, 0, false),
            new (Stats.AmmunitionAmount, 0, false),
            new (Stats.IsInSafezone, 1, false),
            new (Stats.Resets, 0, false),
        });
        mock.Setup(c => c.AttributeCombinations).Returns(new List<AttributeRelationship>
        {
            new (Stats.ArcheryAttackMode, 1, Stats.IsBowEquipped),
            new (Stats.ArcheryAttackMode, 1, Stats.IsCrossBowEquipped),
        });
        mock.Setup(c => c.BaseAttributeValues).Returns(new List<ConstValueAttribute>
        {
            new (1, Stats.SkillMultiplier),
            new (1, Stats.AttackDamageIncrease),
            new (1, Stats.DamageReceiveDecrement),
        });
        return mock;
    }

    private ItemDefinition CreateWeaponDefinition(Player player, byte weaponNumber, byte weaponSlot, AttributeDefinition isBowEquippedFlag)
    {
        var definitionMock = new Mock<ItemDefinition>();
        definitionMock.SetupAllProperties();
        definitionMock.Setup(d => d.QualifiedCharacters).Returns(new List<CharacterClass>());
        definitionMock.Setup(d => d.PossibleItemOptions).Returns(new List<ItemOptionDefinition>());
        definitionMock.Setup(d => d.PossibleItemSetGroups).Returns(new List<ItemSetGroup>());
        definitionMock.Setup(d => d.DropItems).Returns(new List<ItemDropItemGroup>());
        definitionMock.Setup(d => d.Requirements).Returns(new List<AttributeRequirement>());

        var powerUps = new List<ItemBasePowerUpDefinition>
        {
            CreateBasePowerUp(Stats.AmmunitionConsumptionRate, 1f, AggregateType.AddRaw),
            CreateBasePowerUp(isBowEquippedFlag, 1f, AggregateType.AddRaw),
        };
        definitionMock.Setup(d => d.BasePowerUpAttributes).Returns(powerUps);

        var slotTypeMock = new Mock<ItemSlotType>();
        slotTypeMock.Setup(s => s.ItemSlots).Returns(new List<int> { weaponSlot });
        definitionMock.Setup(d => d.ItemSlot).Returns(slotTypeMock.Object);

        var definition = definitionMock.Object;
        definition.Group = BowsGroup;
        definition.Number = weaponNumber;
        definition.Width = 2;
        definition.Height = 3;
        definition.Durability = 100;
        definition.IsAmmunition = false;

        return definition;
    }

    private ItemDefinition CreateAmmoDefinition(Player player, byte ammoNumber, byte ammoSlot)
    {
        var definitionMock = new Mock<ItemDefinition>();
        definitionMock.SetupAllProperties();
        definitionMock.Setup(d => d.QualifiedCharacters).Returns(new List<CharacterClass>());
        definitionMock.Setup(d => d.PossibleItemOptions).Returns(new List<ItemOptionDefinition>());
        definitionMock.Setup(d => d.PossibleItemSetGroups).Returns(new List<ItemSetGroup>());
        definitionMock.Setup(d => d.DropItems).Returns(new List<ItemDropItemGroup>());
        definitionMock.Setup(d => d.Requirements).Returns(new List<AttributeRequirement>());
        definitionMock.Setup(d => d.BasePowerUpAttributes).Returns(new List<ItemBasePowerUpDefinition>());

        var slotTypeMock = new Mock<ItemSlotType>();
        slotTypeMock.Setup(s => s.ItemSlots).Returns(new List<int> { ammoSlot });
        definitionMock.Setup(d => d.ItemSlot).Returns(slotTypeMock.Object);

        var definition = definitionMock.Object;
        definition.Group = BowsGroup;
        definition.Number = ammoNumber;
        definition.Width = 1;
        definition.Height = 1;
        definition.Durability = 100;
        definition.IsAmmunition = true;

        return definition;
    }

    private static ItemBasePowerUpDefinition CreateBasePowerUp(AttributeDefinition target, float value, AggregateType aggregateType)
    {
        var powerUp = new ItemBasePowerUpDefinition
        {
            TargetAttribute = target,
            BaseValue = value,
            AggregateType = aggregateType,
        };
        return powerUp;
    }

    private static Item CreateItem(ItemDefinition definition, double durability)
    {
        var itemMock = new Mock<Item>();
        itemMock.SetupAllProperties();
        itemMock.Setup(i => i.ItemOptions).Returns(new List<ItemOptionLink>());
        itemMock.Setup(i => i.ItemSetGroups).Returns(new List<ItemOfItemSet>());
        var item = itemMock.Object;
        item.Definition = definition;
        item.Durability = durability;
        return item;
    }

    private async ValueTask<Monster> CreateAdjacentMonsterAsync(Player player)
    {
        var monsterDefinition = new MonsterDefinition { ObjectKind = NpcObjectKind.Monster };
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.Level, Value = 10 });
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MinimumPhysBaseDmg, Value = 1 });
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumPhysBaseDmg, Value = 2 });
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.DefenseBase, Value = 0 });
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.MaximumHealth, Value = 10000 });
        monsterDefinition.Attributes.Add(new MonsterAttribute { AttributeDefinition = Stats.AttackRatePvm, Value = 10 });

        var map = await this._gameContext.GetMapAsync(0).ConfigureAwait(false)!;
        var spawnArea = new MonsterSpawnArea
        {
            MonsterDefinition = monsterDefinition,
            GameMap = map!.Definition,
            X1 = player.Position.X,
            Y1 = player.Position.Y,
            X2 = player.Position.X,
            Y2 = player.Position.Y,
            Quantity = 1,
        };

        var monster = new Monster(
            spawnArea,
            monsterDefinition,
            map,
            NullDropGenerator.Instance,
            new Mock<INpcIntelligence>().Object,
            this._gameContext.PlugInManager,
            this._gameContext.PathFinderPool);

        monster.Initialize();
        monster.Attributes[Stats.CurrentHealth] = 10000;
        return monster;
    }
}