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
        (0, 2, 150, 158, 50, 58, 7),
        (0, 3, 195, 203, 120, 128, 7),
        (0, 0, 205, 213, 50, 58, 7),
        (0, 1, 45, 53, 100, 108, 7),

        // Noria: starter and early progression spots.
        (3, 26, 175, 183, 50, 58, 7),
        (3, 27, 210, 218, 70, 78, 7),
        (3, 28, 70, 78, 175, 183, 7),
        (3, 29, 150, 158, 200, 208, 7),

        // Devias: the first mid-level spots. Yeti and Elite Yeti are pulled apart
        // so their boxes no longer touch and merge into a single crowded mass.
        (2, 21, 30, 38, 20, 28, 7),
        (2, 22, 60, 68, 70, 78, 7),
        (2, 19, 195, 203, 200, 208, 7),
        (2, 20, 225, 233, 225, 233, 7),

        // Dungeon: concentrated routes instead of isolated single spawns.
        (1, 8, 40, 48, 115, 123, 7),
        (1, 14, 100, 108, 215, 223, 7),
        (1, 11, 135, 143, 205, 213, 7),
        (1, 17, 230, 238, 165, 173, 7),

        // Lost Tower: compact spots for the next progression range.
        (4, 40, 5, 13, 95, 103, 7),
        (4, 36, 190, 198, 120, 128, 7),
        (4, 39, 232, 240, 120, 128, 7),
        (4, 41, 120, 128, 230, 238, 7),
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
