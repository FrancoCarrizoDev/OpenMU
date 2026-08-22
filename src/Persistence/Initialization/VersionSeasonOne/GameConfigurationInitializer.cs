// <copyright file="GameConfigurationInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using System.Reflection;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;
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
/// ADR-0001 in docs/adr): same maps, NPCs, items and events, except it uses a
/// <see cref="CharacterClassInitialization"/> without Master classes / Summoner / Rage Fighter.
///
/// Harmony and sockets are NOT actually removable by just skipping their initializers or
/// trimming <see cref="OptionTypes"/>: VersionSeasonSix.Items.Weapons and HarmonyOptions itself
/// look up the Harmony option definitions/types with .Single()/.First(), so they hard-crash at
/// startup if those aren't present (see docs/backlog.md). So, like season6, we keep the full
/// option type catalog and still initialize Harmony - what's actually out of scope for Season 1
/// (no Jewel of Harmony craftable, see CreateJewelMixes) is whether players can ever obtain it,
/// not whether the definitions structurally exist. Truly stripping it out of weapons/armor would
/// require forking VersionSeasonSix.Items.Weapons, tracked as a separate backlog item.
/// </summary>
public class GameConfigurationInitializer : GameConfigurationInitializerBase
{
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
        // creates (it looks them up with .Single(), which throws if they don't exist) - see
        // docs/backlog.md. So this has to run even though we don't want Harmony to be a thing
        // in Season 1; it only gets attached to weapons as a result, not to armor (Armors.cs
        // looks the option up defensively with FirstOrDefault). Fully removing Harmony from
        // weapons needs its own Weapons.cs fork - tracked as a separate backlog item.
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
        new DevilSquareInitializer(this.Context, this.GameConfiguration).Initialize();
        new BloodCastleInitializer(this.Context, this.GameConfiguration).Initialize();
        new ChaosCastleInitializer(this.Context, this.GameConfiguration).Initialize();
        new CastleSiegeInitializer(this.Context, this.GameConfiguration).Initialize();
    }

    /// <summary>
    /// Wires the Rena <see cref="DropItemGroup"/> (created in <see cref="Misc"/>) to the Blood Castle
    /// and Devil Square 5-7 event maps, so it only drops there instead of everywhere.
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
            .Where(m => m.Name.Value?.StartsWith("Blood Castle") is true || m.Name.Value is "Devil Square 5" or "Devil Square 6" or "Devil Square 7");
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
