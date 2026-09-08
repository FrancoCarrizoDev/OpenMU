// <copyright file="GameConfigurationInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using System.Reflection;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.ItemCrafting;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Events;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonSix.Items;

/// <summary>
/// Initializes the <see cref="GameConfiguration"/> for the classic "Season 1" style server.
///
/// This is a pruned copy of <see cref="VersionSeasonSix.GameConfigurationInitializer"/> (see
/// ADR-0001/ADR-0004 in docs/adr): shared NPCs, items and event initializers plus the selected map roster,
/// except it uses a
/// <see cref="CharacterClassInitialization"/> without Master classes / Summoner / Rage Fighter.
///
/// Harmony definitions must still be initialized before the shared Season 6 weapon initializer;
/// <see cref="VersionSeasonSix.Items.Weapons"/> resolves its Harmony definitions with
/// <c>Single()</c>, and <see cref="HarmonyOptions"/> resolves its option type with <c>First()</c>.
/// Season 1 therefore initializes those shared definitions and removes their item-facing paths
/// after all shared item, drop and crafting initializers have run.
/// </summary>
public class GameConfigurationInitializer : GameConfigurationInitializerBase
{
    private const short MaximumLevel = 400;
    private const short VipMaximumLevel = 390;
    private const float ExperienceRate = 10.0f;
    private const double ItemDropRate = 0.5;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameConfigurationInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public GameConfigurationInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<ItemOptionType> OptionTypes
    {
        get
        {
            // Same as season6 - see the class remarks on why this can't be trimmed without
            // forking the files that reuse these option types.
            return typeof(ItemOptionTypes)
                .GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(p => p.PropertyType == typeof(ItemOptionType))
                .Select(p => p.GetValue(typeof(ItemOptionType)))
                .OfType<ItemOptionType>()
                .ToList();
        }
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        this.GameConfiguration.MaximumLevel = MaximumLevel;
        this.GameConfiguration.ExperienceRate = ExperienceRate;
        this.GameConfiguration.GlobalBaseAttributeValues.Add(
            this.Context.CreateNew<ConstValueAttribute>(VipMaximumLevel, Stats.VipMaximumLevel.GetPersistent(this.GameConfiguration)));

        this.GameConfiguration.ItemOptions.Add(this.CreateOptionDefinition(Stats.BaseDamageBonus, ItemOptionDefinitionNumbers.PhysicalAndWizardryAttack));
        this.GameConfiguration.ItemOptions.Add(this.CreateOptionDefinition(Stats.CurseBaseDmg, ItemOptionDefinitionNumbers.CurseAttack));

        var maximumAllianceSizeDef = Stats.MaximumAllianceSize.GetPersistent(this.GameConfiguration);
        var maximumAllianceSizeValue = this.Context.CreateNew<ConstValueAttribute>(5f, maximumAllianceSizeDef);
        this.GameConfiguration.GlobalBaseAttributeValues.Add(maximumAllianceSizeValue);

        new CharacterClassInitialization(this.Context, this.GameConfiguration).Initialize();
        new VersionSeasonSix.SkillsInitializer(this.Context, this.GameConfiguration).Initialize();
        new Orbs(this.Context, this.GameConfiguration).Initialize();
        new Scrolls(this.Context, this.GameConfiguration).Initialize();
        new EventTicketItems(this.Context, this.GameConfiguration).Initialize();
        new Wings(this.Context, this.GameConfiguration).Initialize();
        new Pets(this.Context, this.GameConfiguration).Initialize();
        new ExcellentOptions(this.Context, this.GameConfiguration).Initialize();

        // VersionSeasonSix.Items.Weapons hard-requires the option definitions HarmonyOptions
        // creates (it looks them up with .Single(), which throws if they don't exist). Keep
        // this initializer before Weapons, then detach the definitions from Season 1 items in
        // DisableHarmonyAndSockets after all shared initializers have completed.
        new HarmonyOptions(this.Context, this.GameConfiguration).Initialize();

        new GuardianOptions(this.Context, this.GameConfiguration).Initialize();
        new Armors(this.Context, this.GameConfiguration).Initialize();
        new Weapons(this.Context, this.GameConfiguration).Initialize();
        new Potions(this.Context, this.GameConfiguration).Initialize();
        new Jewels(this.Context, this.GameConfiguration).Initialize();
        new Misc(this.Context, this.GameConfiguration).Initialize();
        new PackedJewels(this.Context, this.GameConfiguration).Initialize();
        new Jewelery(this.Context, this.GameConfiguration).Initialize();
        new AncientSets(this.Context, this.GameConfiguration).Initialize();
        new BoxOfLuck(this.Context, this.GameConfiguration).Initialize();
        this.ExcludeItemsForUnavailableClassesFromMonsterDrops();
        this.CreateJewelMixes();
        new NpcInitialization(this.Context, this.GameConfiguration).Initialize();
        new InvasionMobsInitialization(this.Context, this.GameConfiguration).Initialize();
        new GameMapsInitializer(this.Context, this.GameConfiguration).Initialize();
        this.AssignCharacterClassHomeMaps();
        this.WireRenaDropToEventMaps();
        new ChaosMixes(this.Context, this.GameConfiguration).Initialize();
        new Gates(this.Context, this.GameConfiguration).Initialize();
        new Quest(this.Context, this.GameConfiguration).Initialize();
        new Quests(this.Context, this.GameConfiguration).Initialize();
        new Version095d.Events.DevilSquareInitializer(this.Context, this.GameConfiguration).Initialize();
        new BloodCastleInitializer(this.Context, this.GameConfiguration).Initialize();
        new Events.ChaosCastleInitializer(this.Context, this.GameConfiguration).Initialize();
        this.ApplyItemDropRate();
        this.DisableHarmonyAndSockets();
    }

    /// <summary>
    /// Wires the Rena <see cref="DropItemGroup"/> (created in <see cref="Misc"/>) to the Blood Castle
    /// event maps, so it only drops there instead of everywhere.
    /// </summary>
    private void WireRenaDropToEventMaps()
    {
        var renaDropGroup = this.GameConfiguration.DropItemGroups
            .FirstOrDefault(g => g.PossibleItems.Any(i => i.Group == 14 && i.Number == 21));
        if (renaDropGroup is null)
        {
            return;
        }

        var eventMaps = this.GameConfiguration.Maps
            .Where(m => m.Name.Value?.StartsWith("Blood Castle") is true);
        foreach (var map in eventMaps)
        {
            if (!map.DropItemGroups.Contains(renaDropGroup))
            {
                map.DropItemGroups.Add(renaDropGroup);
            }
        }
    }

    private void CreateJewelMixes()
    {
        this.CreateJewelMix(0, 13, 0xE, 30); // Bless
        this.CreateJewelMix(1, 14, 0xE, 31); // Soul
        this.CreateJewelMix(2, 16, 0xE, 136); // Jewel of Life
        this.CreateJewelMix(3, 22, 0xE, 137); // Jewel of Creation
        this.CreateJewelMix(4, 31, 0xE, 138); // Jewel of Guardian
        this.CreateJewelMix(5, 41, 0xE, 139); // Gemstone
        this.CreateJewelMix(7, 15, 0xC, 141); // Chaos
        this.CreateJewelMix(8, 43, 0xE, 142); // Lower Refine Stone
        this.CreateJewelMix(9, 44, 0xE, 143); // Higher Refine Stone

        // Jewel of Harmony (mix number 6) intentionally skipped - out of scope for Season 1.
    }

    private void ApplyItemDropRate()
    {
        foreach (var dropGroup in this.GameConfiguration.DropItemGroups.Where(group => group.Chance > 0 && group.Chance < 1))
        {
            dropGroup.Chance *= ItemDropRate;
        }
    }

    /// <summary>
    /// Removes the Harmony and socket paths added by shared Season 6 initializers.
    ///
    /// The shared weapon initializer must see the Harmony definitions while it runs, but Season 1
    /// must not expose those definitions through random options, drops or crafting. SocketSystem
    /// is not initialized for Season 1; the extra cleanup also makes this boundary resilient if a
    /// future shared initializer adds socket content before this method is called.
    /// </summary>
    private void DisableHarmonyAndSockets()
    {
        foreach (var item in this.GameConfiguration.Items)
        {
            item.MaximumSockets = 0;
            foreach (var optionDefinition in item.PossibleItemOptions
                         .Where(this.ContainsHarmonyOrSocketOption)
                         .ToList())
            {
                item.PossibleItemOptions.Remove(optionDefinition);
            }
        }

        foreach (var dropGroup in this.GetAllDropGroups())
        {
            foreach (var item in dropGroup.PossibleItems
                         .Where(this.IsHarmonyOrSocketItem)
                         .ToList())
            {
                dropGroup.PossibleItems.Remove(item);
            }
        }

        foreach (var monster in this.GameConfiguration.Monsters)
        {
            foreach (var crafting in monster.ItemCraftings
                         .Where(this.ContainsHarmonyOrSocketContent)
                         .ToList())
            {
                monster.ItemCraftings.Remove(crafting);
            }
        }
    }

    private IEnumerable<DropItemGroup> GetAllDropGroups()
    {
        foreach (var dropGroup in this.GameConfiguration.DropItemGroups)
        {
            yield return dropGroup;
        }

        foreach (var map in this.GameConfiguration.Maps)
        {
            foreach (var dropGroup in map.DropItemGroups)
            {
                yield return dropGroup;
            }
        }

        foreach (var monster in this.GameConfiguration.Monsters)
        {
            foreach (var dropGroup in monster.DropItemGroups)
            {
                yield return dropGroup;
            }
        }

        foreach (var item in this.GameConfiguration.Items)
        {
            foreach (var dropGroup in item.DropItems)
            {
                yield return dropGroup;
            }
        }
    }

    private bool ContainsHarmonyOrSocketOption(ItemOptionDefinition definition)
    {
        return definition.PossibleOptions.Any(option => this.IsHarmonyOrSocketOption(option.OptionType));
    }

    private bool IsHarmonyOrSocketOption(ItemOptionType? optionType)
    {
        return optionType == ItemOptionTypes.HarmonyOption
               || optionType == ItemOptionTypes.SocketOption
               || optionType == ItemOptionTypes.SocketBonusOption;
    }

    private bool IsHarmonyOrSocketItem(ItemDefinition item)
    {
        return (item.Group == 14 && item.Number == 42) // Jewel of Harmony
               || (item.Group == 12 && item.Number == 140) // Packed Jewel of Harmony
               || (item.Group == 12 && item.Number is >= 60 and <= 75) // Seeds and spheres
               || (item.Group == 12 && item.Number is >= 100 and <= 129); // Seed spheres
    }

    private bool ContainsHarmonyOrSocketContent(ItemCrafting crafting)
    {
        var settings = crafting.SimpleCraftingSettings;
        if (settings is null)
        {
            return false;
        }

        return settings.RequiredItems.Any(requiredItem =>
                   requiredItem.PossibleItems.Any(this.IsHarmonyOrSocketItem)
                   || requiredItem.RequiredItemOptions.Any(this.IsHarmonyOrSocketOption))
               || settings.ResultItems.Any(resultItem => resultItem.ItemDefinition is { } item && this.IsHarmonyOrSocketItem(item));
    }

    private void ExcludeItemsForUnavailableClassesFromMonsterDrops()
    {
        // Missing classes are omitted from QualifiedCharacters during item initialization.
        foreach (var item in this.GameConfiguration.Items.Where(item => item.DropsFromMonsters
                                                                        && item.ItemSlot is not null
                                                                        && !item.QualifiedCharacters.Any()))
        {
            item.DropsFromMonsters = false;
        }
    }

    private void CreateJewelMix(byte mixNumber, int itemNumber, int itemGroup, int packedJewelId)
    {
        var singleJewel = this.GameConfiguration.Items.First(i => i.Group == itemGroup && i.Number == itemNumber);
        var packedJewel = this.GameConfiguration.Items.First(i => i.Group == 0x0C && i.Number == packedJewelId);
        var jewelMix = this.Context.CreateNew<JewelMix>();
        jewelMix.SetGuid(mixNumber);
        jewelMix.Number = mixNumber;
        jewelMix.SingleJewel = singleJewel;
        jewelMix.MixedJewel = packedJewel;
        this.GameConfiguration.JewelMixes.Add(jewelMix);
    }
}
