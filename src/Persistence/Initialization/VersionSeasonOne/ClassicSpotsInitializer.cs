// <copyright file="ClassicSpotsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Adds compact classic monster spots to the early progression maps.
/// </summary>
internal sealed class ClassicSpotsInitializer : InitializerBase
{
    private const short FirstSpotNumber = 9000;

    private static readonly (byte MapNumber, short MonsterNumber, byte X1, byte X2, byte Y1, byte Y2, short Quantity)[] Spots =
    {
        // Lorencia: starter spots.
        (0, 2, 150, 160, 50, 60, 8),
        (0, 3, 195, 205, 120, 130, 8),
        (0, 0, 205, 215, 50, 60, 8),
        (0, 1, 45, 55, 100, 110, 8),

        // Noria: starter and early progression spots.
        (3, 26, 175, 185, 50, 60, 8),
        (3, 27, 210, 220, 70, 80, 8),
        (3, 28, 70, 80, 175, 185, 8),
        (3, 29, 150, 160, 200, 210, 8),

        // Devias: the first mid-level spots.
        (2, 21, 30, 40, 20, 30, 8),
        (2, 22, 60, 70, 70, 80, 8),
        (2, 19, 205, 215, 215, 225, 8),
        (2, 20, 220, 230, 215, 225, 8),

        // Dungeon: concentrated routes instead of isolated single spawns.
        (1, 8, 40, 50, 115, 125, 8),
        (1, 14, 100, 110, 215, 225, 8),
        (1, 11, 135, 145, 205, 215, 8),
        (1, 17, 230, 240, 165, 175, 8),

        // Lost Tower: compact spots for the next progression range.
        (4, 40, 5, 15, 95, 105, 8),
        (4, 36, 190, 200, 120, 130, 8),
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ClassicSpotsInitializer"/> class.
    /// </summary>
    /// <param name="context">The persistence context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public ClassicSpotsInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        for (var index = 0; index < Spots.Length; index++)
        {
            var definition = Spots[index];
            var map = this.GameConfiguration.Maps.Single(
                candidate => candidate.Number == definition.MapNumber && candidate.Discriminator == 0);
            var monster = this.GameConfiguration.Monsters.Single(candidate => candidate.Number == definition.MonsterNumber);
            var spawn = this.Context.CreateNew<MonsterSpawnArea>();
            spawn.SetGuid(map.Number, (short)(FirstSpotNumber + index));
            spawn.GameMap = map;
            spawn.MonsterDefinition = monster;
            spawn.X1 = definition.X1;
            spawn.X2 = definition.X2;
            spawn.Y1 = definition.Y1;
            spawn.Y2 = definition.Y2;
            spawn.Quantity = definition.Quantity;
            spawn.Direction = Direction.Undefined;
            spawn.SpawnTrigger = SpawnTrigger.Automatic;
            map.MonsterSpawns.Add(spawn);
        }
    }
}
