// <copyright file="ResetSeasonOneTestAccountsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.DataModel.Configuration.Items;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.Persistence.Initialization.CharacterClasses;
using MUnique.OpenMU.Persistence.Initialization.Items;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Replaces the test0-test9 accounts' characters with a clean level 1 roster (Dark Knight,
/// Dark Wizard, Fairy Elf, Dark Lord, Magic Gladiator), each carrying only the same starting
/// gear a freshly created character gets in-game (see the <c>CharacterCreated</c> item
/// plug-ins), no other inventory items, no vault items and no zen. Replaces the old QA seed
/// characters (levels 1/11/21/.../91 with test gear and starter zen), which friends testing
/// the server found confusing to receive as "new" accounts.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("6E3F9F0B-6A66-4C7A-9E9C-2C6C4C7E9B21")]
public sealed class ResetSeasonOneTestAccountsPlugIn : UpdatePlugInBase
{
    private const string PlugInName = "Reset Season 1 Test Accounts";
    private const string PlugInDescription = "Gives test0-test9 a clean level 1 Dk/Dw/Elf/Dl/Mg roster with only default starting gear, no vault items and no zen.";

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.ResetSeasonOneTestAccounts;

    /// <inheritdoc />
    public override string DataInitializationKey => DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 9, 12, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var testAccountNames = Enumerable.Range(0, 10).Select(i => "test" + i).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var allAccounts = await context.GetAsync<Account>().ConfigureAwait(false);
        var testAccountIds = allAccounts.Where(a => testAccountNames.Contains(a.LoginName)).Select(a => a.GetId()).ToList();
        if (testAccountIds.Count == 0)
        {
            return;
        }

        var darkKnight = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)CharacterClassNumber.DarkKnight);
        var darkWizard = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)CharacterClassNumber.DarkWizard);
        var fairyElf = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)CharacterClassNumber.FairyElf);
        var darkLord = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)CharacterClassNumber.DarkLord);
        var magicGladiator = gameConfiguration.CharacterClasses.First(c => c.Number == (byte)CharacterClassNumber.MagicGladiator);

        var smallAxe = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Axes && i.Number == 0);
        var shortSword = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Swords && i.Number == 1);
        var smallShield = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Shields && i.Number == 0);
        var shortBow = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Bows && i.Number == 0);
        var arrows = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Bows && i.Number == 15);
        var ringOfWarrior = gameConfiguration.Items.First(i => i.Group == (byte)ItemGroups.Misc1 && i.Number == 20);

        foreach (var accountId in testAccountIds)
        {
            if (await context.GetByIdAsync<Account>(accountId).ConfigureAwait(false) is not { } account)
            {
                continue;
            }

            foreach (var character in account.Characters.ToList())
            {
                account.Characters.Remove(character);
                await context.DeleteAsync(character).ConfigureAwait(false);
            }

            if (account.Vault is { } vault)
            {
                foreach (var item in vault.Items.ToList())
                {
                    vault.Items.Remove(item);
                    await context.DeleteAsync(item).ConfigureAwait(false);
                }
            }

            // Commit the removals before inserting the new characters below, which reuse the
            // same names (e.g. "test0Dk") and would otherwise collide with the rows being
            // deleted in the same save.
            await context.SaveChangesAsync().ConfigureAwait(false);

            var ringOfWarriorPlusOne = InventoryConstants.LastEquippableItemSlotIndex + 1;
            var ringOfWarriorPlusTwo = InventoryConstants.LastEquippableItemSlotIndex + 2;

            account.Characters.Add(this.CreateCharacter(context, account.LoginName + "Dk", darkKnight, 0, new[]
            {
                this.CreateItem(context, smallAxe, 0),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusOne, level: 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusTwo, level: 2),
            }));

            account.Characters.Add(this.CreateCharacter(context, account.LoginName + "Dw", darkWizard, 1, new[]
            {
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusOne, level: 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusTwo, level: 2),
            }));

            account.Characters.Add(this.CreateCharacter(context, account.LoginName + "Elf", fairyElf, 2, new[]
            {
                this.CreateItem(context, arrows, 0, durability: 255),
                this.CreateItem(context, shortBow, 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusOne, level: 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusTwo, level: 2),
            }));

            account.Characters.Add(this.CreateCharacter(context, account.LoginName + "Dl", darkLord, 3, new[]
            {
                this.CreateItem(context, shortSword, 0),
                this.CreateItem(context, smallShield, 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusOne, level: 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusTwo, level: 2),
            }));

            account.Characters.Add(this.CreateCharacter(context, account.LoginName + "Mg", magicGladiator, 4, new[]
            {
                this.CreateItem(context, shortSword, 0),
                this.CreateItem(context, smallShield, 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusOne, level: 1),
                this.CreateItem(context, ringOfWarrior, (byte)ringOfWarriorPlusTwo, level: 2),
            }));

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
    }

    private Character CreateCharacter(IContext context, string name, CharacterClass characterClass, byte slot, IEnumerable<Item> items)
    {
        var character = context.CreateNew<Character>();
        character.CharacterClass = characterClass;
        character.Name = name;
        character.CharacterSlot = slot;
        character.CreateDate = DateTime.UtcNow;
        character.KeyConfiguration = new byte[30];
        foreach (var attribute in characterClass.StatAttributes.Select(a => context.CreateNew<StatAttribute>(a.Attribute, a.BaseValue)))
        {
            character.Attributes.Add(attribute);
        }

        character.CurrentMap = characterClass.HomeMap;
        var spawnGate = character.CurrentMap!.ExitGates.Where(g => g.IsSpawnGate).SelectRandom();
        if (spawnGate is not null)
        {
            character.PositionX = (byte)Rand.NextInt(spawnGate.X1, spawnGate.X2);
            character.PositionY = (byte)Rand.NextInt(spawnGate.Y1, spawnGate.Y2);
        }

        character.Attributes.First(a => a.Definition == Stats.Level).Value = 1;
        character.Inventory = context.CreateNew<ItemStorage>();
        character.Inventory.Money = 0;
        foreach (var item in items)
        {
            character.Inventory.Items.Add(item);
        }

        return character;
    }

    private Item CreateItem(IContext context, ItemDefinition definition, byte slot, byte level = 0, byte? durability = null)
    {
        var item = context.CreateNew<Item>();
        item.Definition = definition;
        item.Durability = durability ?? definition.Durability;
        item.ItemSlot = slot;
        item.Level = level;
        return item;
    }
}
