// <copyright file="Tarkan.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// The Season 1 Classic variant of Tarkan, with its pre-reset merchant.
/// </summary>
internal class Tarkan : Version095d.Maps.Tarkan
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Tarkan"/> class.
    /// </summary>
    public Tarkan(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc/>
    protected override IEnumerable<MonsterSpawnArea> CreateNpcSpawns()
    {
        // Bolo is configured by MerchantStores and is placed next to the primary warp arrival
        // area (187-203, 63-69), outside Tarkan's monster packs.
        yield return this.CreateMonsterSpawn(1, this.NpcDictionary[578], 194, 75, Direction.SouthWest);
    }
}
