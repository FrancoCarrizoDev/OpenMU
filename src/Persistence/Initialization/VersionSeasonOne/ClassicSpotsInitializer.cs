// <copyright file="ClassicSpotsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Adds compact classic monster spots to the early progression maps.
/// </summary>
/// <remarks>
/// Each monster of a spot gets its own fixed origin point (a <see cref="MonsterSpawnArea"/> with
/// <see cref="MonsterSpawnArea.X1"/> == <see cref="MonsterSpawnArea.X2"/> and
/// <see cref="MonsterSpawnArea.Y1"/> == <see cref="MonsterSpawnArea.Y2"/>) instead of sharing one area
/// with a random position, so it always spawns and respawns at the same coordinate. The points were
/// chosen to fill the same bounding box, at the same density, that a single random area spot used to
/// cover, and were validated against the real map terrain to ensure they're walkable.
/// </remarks>
internal sealed class ClassicSpotsInitializer : InitializerBase
{
    private const short FirstSpotNumber = 9000;

    private static readonly (byte MapNumber, short MonsterNumber, (byte X, byte Y)[] Points)[] Spots =
    {
        // Lorencia: starter spots.
        (0, 2, new (byte X, byte Y)[] { (150, 50), (154, 50), (158, 50), (150, 54), (154, 54), (158, 54), (150, 58) }),
        (0, 3, new (byte X, byte Y)[] { (195, 120), (199, 120), (203, 120), (195, 124), (199, 124), (203, 124), (195, 128) }),
        (0, 0, new (byte X, byte Y)[] { (205, 50), (209, 50), (213, 50), (205, 54), (209, 54), (213, 54), (205, 58) }),
        (0, 1, new (byte X, byte Y)[] { (45, 100), (49, 100), (53, 100), (45, 104), (49, 104), (53, 104), (45, 108) }),

        // Noria: starter and early progression spots.
        (3, 26, new (byte X, byte Y)[] { (175, 50), (179, 50), (183, 50), (175, 54), (179, 54), (183, 54), (175, 58) }),
        (3, 27, new (byte X, byte Y)[] { (210, 70), (214, 70), (218, 70), (210, 74), (214, 74), (218, 74), (210, 78) }),
        (3, 28, new (byte X, byte Y)[] { (70, 175), (74, 175), (78, 175), (70, 178), (74, 179), (78, 179), (70, 183) }),
        (3, 29, new (byte X, byte Y)[] { (150, 200), (154, 200), (158, 200), (150, 204), (154, 204), (158, 204), (150, 208) }),

        // Devias: the first mid-level spots. Yeti and Elite Yeti are pulled apart
        // so their boxes no longer touch and merge into a single crowded mass.
        (2, 21, new (byte X, byte Y)[] { (30, 20), (34, 20), (38, 20), (30, 24), (34, 24), (38, 24), (30, 27) }),
        (2, 22, new (byte X, byte Y)[] { (62, 70), (64, 70), (68, 70), (60, 74), (64, 74), (68, 74), (60, 77) }),
        (2, 19, new (byte X, byte Y)[] { (196, 200), (199, 200), (203, 200), (196, 203), (199, 204), (203, 204), (195, 208) }),
        (2, 20, new (byte X, byte Y)[] { (225, 225), (229, 225), (231, 225), (225, 229), (229, 229), (231, 227), (225, 233) }),

        // Dungeon: concentrated routes instead of isolated single spawns.
        (1, 8, new (byte X, byte Y)[] { (40, 115), (44, 115), (46, 115), (40, 119), (44, 119), (46, 117), (40, 123) }),
        (1, 14, new (byte X, byte Y)[] { (100, 215), (104, 215), (108, 215), (100, 219), (104, 219), (108, 219), (100, 223) }),
        (1, 11, new (byte X, byte Y)[] { (135, 205), (139, 205), (143, 205), (135, 208), (139, 209), (143, 209), (135, 213) }),
        (1, 17, new (byte X, byte Y)[] { (230, 166), (233, 166), (238, 165), (230, 169), (234, 169), (238, 169), (230, 172) }),

        // Lost Tower: compact spots for the next progression range.
        (4, 40, new (byte X, byte Y)[] { (5, 95), (9, 95), (13, 95), (5, 99), (9, 99), (13, 99), (5, 103) }),
        (4, 36, new (byte X, byte Y)[] { (190, 121), (193, 121), (197, 121), (190, 124), (194, 124), (198, 124), (190, 128) }),
        (4, 39, new (byte X, byte Y)[] { (232, 120), (236, 120), (240, 120), (232, 124), (236, 124), (240, 124), (232, 128) }),
        (4, 41, new (byte X, byte Y)[] { (120, 230), (124, 230), (128, 230), (120, 234), (124, 234), (128, 234), (120, 238) }),

        // Lost Tower: additional Elf Soldier requested for the classic route.
        (4, 257, new (byte X, byte Y)[] { (202, 80) }),
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
        var spotNumber = FirstSpotNumber;
        foreach (var definition in Spots)
        {
            var map = this.GameConfiguration.Maps.Single(
                candidate => candidate.Number == definition.MapNumber && candidate.Discriminator == 0);
            var monster = this.GameConfiguration.Monsters.Single(candidate => candidate.Number == definition.MonsterNumber);

            foreach (var point in definition.Points)
            {
                var spawn = this.Context.CreateNew<MonsterSpawnArea>();
                spawn.SetGuid(map.Number, spotNumber);
                spotNumber++;
                spawn.GameMap = map;
                spawn.MonsterDefinition = monster;
                spawn.X1 = point.X;
                spawn.X2 = point.X;
                spawn.Y1 = point.Y;
                spawn.Y2 = point.Y;
                spawn.Quantity = 1;
                spawn.Direction = Direction.Undefined;
                spawn.SpawnTrigger = SpawnTrigger.Automatic;
                map.MonsterSpawns.Add(spawn);
            }
        }
    }
}
