// <copyright file="MerchantCatalogSeasonOneTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Guards the NPC merchant catalog against content which is not usable in season1-classic.
/// </summary>
[TestFixture]
internal sealed class MerchantCatalogSeasonOneTests
{
    /// <summary>
    /// Verifies that merchant stores do not expose unavailable-class equipment or excluded
    /// Illusion Temple / Summoner skill items.
    /// </summary>
    [Test]
    public async Task MerchantStoresContainOnlySeasonOneClassicContentAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new VersionSeasonOne.DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);

        using var context = contextProvider.CreateNewConfigurationContext();
        var configuration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).Single();
        var merchantItems = configuration.Monsters
            .Where(monster => monster.MerchantStore is not null)
            .SelectMany(monster => monster.MerchantStore!.Items)
            .Select(item => item.Definition!)
            .ToList();

        var unavailableEquipment = merchantItems
            .Where(item => item.ItemSlot is not null && item.QualifiedCharacters.Count == 0)
            .Select(item => $"{item.Group}:{item.Number} {item.Name.Value}")
            .Distinct()
            .ToList();
        var illusionTempleItems = merchantItems
            .Where(item => item.Group == 13 && item.Number is 49 or 50)
            .Select(item => $"{item.Group}:{item.Number} {item.Name.Value}")
            .Distinct()
            .ToList();
        var summonerSkillItems = merchantItems
            .Where(item => item.Group == 15 && item.Number == 20)
            .Select(item => $"{item.Group}:{item.Number} {item.Name.Value}")
            .Distinct()
            .ToList();

        Assert.Multiple(() =>
        {
            Assert.That(unavailableEquipment, Is.Empty, "Merchant equipment must be usable by at least one Season 1 class.");
            Assert.That(illusionTempleItems, Is.Empty, "Illusion Temple ticket items are post-Season 1 content.");
            Assert.That(summonerSkillItems, Is.Empty, "Drain Life is a Summoner-only skill item.");
        });
    }
}
