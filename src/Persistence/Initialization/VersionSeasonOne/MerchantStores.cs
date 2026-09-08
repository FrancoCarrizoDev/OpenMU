// <copyright file="MerchantStores.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.Persistence.Initialization.Items;

/// <summary>
/// Configures the merchant catalogs which are specific to the classic progression.
/// </summary>
/// <remarks>
/// The regular Season 6 catalogs are intentionally left untouched. This seed uses the
/// same NPC definitions, but its shorter progression needs a smaller number of meaningful
/// upgrades and a vendor in Tarkan before the first reset.
/// </remarks>
internal sealed class MerchantStores : InitializerBase
{
    private readonly ItemHelper _itemHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MerchantStores"/> class.
    /// </summary>
    public MerchantStores(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
        this._itemHelper = new ItemHelper(context, gameConfiguration);
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        this.ConfigureHanzo();
        this.AddDarkLordSkillsToPasi();
        this.ConfigureTarkanMerchant();
    }

    private void ConfigureHanzo()
    {
        // A fast early game does not need five nearly identical starter sets. These are the
        // three usable DK breakpoints before the Dragon set sold in Devias.
        var items = new List<Item>
        {
            this._itemHelper.CreateSetItem(0, 0, ItemGroups.Helm, null, 2, 1, true),
            this._itemHelper.CreateSetItem(2, 0, ItemGroups.Armor, null, 2, 1, true),
            this._itemHelper.CreateSetItem(4, 0, ItemGroups.Pants, null, 2, 1, true),
            this._itemHelper.CreateSetItem(6, 0, ItemGroups.Gloves, null, 2, 1, true),
            this._itemHelper.CreateSetItem(8, 0, ItemGroups.Boots, null, 2, 1, true), // Bronze

            this._itemHelper.CreateSetItem(16, 6, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(18, 6, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(20, 6, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(22, 6, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(24, 6, ItemGroups.Boots, null, 3, 1, true), // Scale

            this._itemHelper.CreateSetItem(32, 9, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(34, 9, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(36, 9, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(38, 9, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(40, 9, ItemGroups.Boots, null, 3, 1, true), // Plate

            this._itemHelper.CreateShield(48, 2, false, null, 3, 1, true), // Kite Shield
            this._itemHelper.CreateShield(50, 7, true, null, 3, 1, true),  // Spiked Shield
            this._itemHelper.CreateShield(52, 8, true, null, 3, 1, true),  // Tower Shield

            this._itemHelper.CreateWeapon(56, ItemGroups.Swords, 4, 3, 1, true, true, null),  // Sword of Assassin
            this._itemHelper.CreateWeapon(58, ItemGroups.Swords, 5, 3, 1, true, true, null),  // Blade
            this._itemHelper.CreateWeapon(60, ItemGroups.Swords, 11, 3, 1, true, true, null), // Legendary Sword
        };

        this.ReplaceStore(251, items);
    }

    private void AddDarkLordSkillsToPasi()
    {
        var pasiStore = this.GetMerchant(254).MerchantStore!;

        // Force is the DL's base skill and Force Wave has no learnable item definition.
        // These are every DL learnable item that exists in this protocol/content set.
        pasiStore.Items.Add(this._itemHelper.CreateOrb(40, 21)); // Scroll of FireBurst
        pasiStore.Items.Add(this._itemHelper.CreateOrb(41, 22)); // Scroll of Summon
        pasiStore.Items.Add(this._itemHelper.CreateOrb(42, 23)); // Scroll of Critical Damage
        pasiStore.Items.Add(this._itemHelper.CreateOrb(43, 24)); // Scroll of Electric Spark
        pasiStore.Items.Add(this._itemHelper.CreateOrb(44, 35)); // Scroll of Fire Scream
    }

    private void ConfigureTarkanMerchant()
    {
        // Bolo is otherwise only spawned in excluded Karutan. Reusing this merchant definition
        // gives Tarkan one focused, pre-reset catalog without adding a client-side NPC asset.
        var items = new List<Item>
        {
            this._itemHelper.CreateSetItem(0, 16, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(2, 16, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(4, 16, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(6, 16, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(8, 16, ItemGroups.Boots, null, 3, 1, true), // Black Dragon (DK)

            this._itemHelper.CreateSetItem(16, 18, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(18, 18, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(20, 18, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(22, 18, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(24, 18, ItemGroups.Boots, null, 3, 1, true), // Grand Soul (DW)

            this._itemHelper.CreateSetItem(32, 19, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(34, 19, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(36, 19, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(38, 19, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(40, 19, ItemGroups.Boots, null, 3, 1, true), // Divine (Elf)

            this._itemHelper.CreateSetItem(48, 27, ItemGroups.Helm, null, 3, 1, true),
            this._itemHelper.CreateSetItem(50, 27, ItemGroups.Armor, null, 3, 1, true),
            this._itemHelper.CreateSetItem(52, 27, ItemGroups.Pants, null, 3, 1, true),
            this._itemHelper.CreateSetItem(54, 27, ItemGroups.Gloves, null, 3, 1, true),
            this._itemHelper.CreateSetItem(56, 27, ItemGroups.Boots, null, 3, 1, true), // Dark Steel (DL)

            this._itemHelper.CreateWeapon(64, ItemGroups.Swords, 16, 3, 1, true, true, null),   // Sword of Destruction
            this._itemHelper.CreateWeapon(66, ItemGroups.Staff, 6, 3, 1, true, false, null),     // Staff of Resurrection
            this._itemHelper.CreateWeapon(68, ItemGroups.Bows, 14, 3, 1, true, true, null),      // Aquagold Crossbow
            this._itemHelper.CreateWeapon(70, ItemGroups.Scepters, 11, 3, 1, true, true, null),  // Lord Scepter
        };

        this.ReplaceStore(578, items);
    }

    private MonsterDefinition GetMerchant(short number)
    {
        return this.GameConfiguration.Monsters.Single(monster => monster.Number == number);
    }

    private void ReplaceStore(short merchantNumber, IEnumerable<Item> items)
    {
        // The NPC initializer already registered this storage with a deterministic GUID.
        // Reusing it avoids creating another entity with the same identifier.
        var store = this.GetMerchant(merchantNumber).MerchantStore!;
        store.Items.Clear();
        foreach (var item in items)
        {
            store.Items.Add(item);
        }
    }
}
