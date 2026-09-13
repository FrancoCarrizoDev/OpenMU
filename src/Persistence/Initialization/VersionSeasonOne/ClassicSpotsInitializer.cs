// <copyright file="ClassicSpotsInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel;
using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Adds compact classic monster spots across the Season 1 map roster.
/// </summary>
/// <remarks>
/// Each monster of a spot gets its own fixed origin point (a <see cref="MonsterSpawnArea"/> with
/// <see cref="MonsterSpawnArea.X1"/> == <see cref="MonsterSpawnArea.X2"/> and
/// <see cref="MonsterSpawnArea.Y1"/> == <see cref="MonsterSpawnArea.Y2"/>) instead of sharing one area
/// with a random position, so it always spawns and respawns at the same coordinate. On the early
/// maps (Lorencia, Noria, Devias, Dungeon, Lost Tower) the points were chosen to fill the same
/// bounding box, at the same density, that a single random area spot used to cover. The mid/high
/// level maps (Atlans onward) already ship with dozens of individually placed monster spawns, so
/// their spots are convenience/known farm points rather than a density fix. Every point was
/// validated against the real map terrain to ensure it's walkable.
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

        // Lorencia: second-tier spots (stronger variants already in the vanilla roster,
        // now with their own compact cluster instead of only the huge wandering area).
        (0, 4, new (byte X, byte Y)[] { (170, 86), (174, 86), (178, 86), (170, 90), (174, 90), (178, 90), (170, 94) }),
        (0, 6, new (byte X, byte Y)[] { (110, 200), (114, 200), (118, 200), (110, 204), (114, 204), (118, 204), (110, 208) }),
        (0, 7, new (byte X, byte Y)[] { (15, 65), (19, 65), (23, 65), (15, 69), (19, 69), (23, 69), (15, 73) }),

        // Noria: starter and early progression spots.
        (3, 26, new (byte X, byte Y)[] { (175, 50), (179, 50), (183, 50), (175, 54), (179, 54), (183, 54), (175, 58) }),
        (3, 27, new (byte X, byte Y)[] { (210, 70), (214, 70), (218, 70), (210, 74), (214, 74), (218, 74), (210, 78) }),
        (3, 28, new (byte X, byte Y)[] { (70, 175), (74, 175), (78, 175), (70, 178), (74, 179), (78, 179), (70, 183) }),
        (3, 29, new (byte X, byte Y)[] { (150, 200), (154, 200), (158, 200), (150, 204), (154, 204), (158, 204), (150, 208) }),

        // Noria: second-tier spots for the higher end of the Noria range.
        (3, 30, new (byte X, byte Y)[] { (30, 180), (34, 180), (38, 180), (30, 184), (34, 184), (38, 184), (30, 188) }),
        (3, 31, new (byte X, byte Y)[] { (200, 180), (204, 180), (208, 180), (200, 184), (204, 184), (208, 184), (200, 188) }),

        // Devias: the first mid-level spots. Yeti and Elite Yeti are pulled apart
        // so their boxes no longer touch and merge into a single crowded mass.
        (2, 21, new (byte X, byte Y)[] { (30, 20), (34, 20), (38, 20), (30, 24), (34, 24), (38, 24), (30, 27) }),
        (2, 22, new (byte X, byte Y)[] { (62, 70), (64, 70), (68, 70), (60, 74), (64, 74), (68, 74), (60, 77) }),
        (2, 19, new (byte X, byte Y)[] { (196, 200), (199, 200), (203, 200), (196, 203), (199, 204), (203, 204), (195, 208) }),
        (2, 20, new (byte X, byte Y)[] { (225, 225), (229, 225), (231, 225), (225, 229), (229, 229), (231, 227), (225, 233) }),

        // Devias: Hommerd and Worm, the two remaining early monster types that had no compact spot yet.
        (2, 23, new (byte X, byte Y)[] { (140, 77), (144, 77), (148, 77), (140, 81), (144, 81), (148, 81), (140, 85) }),
        (2, 24, new (byte X, byte Y)[] { (190, 107), (194, 107), (198, 107), (190, 111), (194, 111), (198, 111), (190, 115) }),

        // Dungeon: concentrated routes instead of isolated single spawns.
        (1, 8, new (byte X, byte Y)[] { (40, 115), (44, 115), (46, 115), (40, 119), (44, 119), (46, 117), (40, 123) }),
        (1, 14, new (byte X, byte Y)[] { (100, 215), (104, 215), (108, 215), (100, 219), (104, 219), (108, 219), (100, 223) }),
        (1, 11, new (byte X, byte Y)[] { (135, 205), (139, 205), (143, 205), (135, 208), (139, 209), (143, 209), (135, 213) }),
        (1, 17, new (byte X, byte Y)[] { (230, 166), (233, 166), (238, 165), (230, 169), (234, 169), (238, 169), (230, 172) }),

        // Dungeon: Gorgon, so the route has a spot for all four dungeon-native mobs named in the guide.
        (1, 18, new (byte X, byte Y)[] { (20, 80), (24, 80), (28, 80), (20, 84), (24, 84), (28, 84), (20, 88) }),

        // Lost Tower: compact spots for the next progression range.
        (4, 40, new (byte X, byte Y)[] { (5, 95), (9, 95), (13, 95), (5, 99), (9, 99), (13, 99), (5, 103) }),
        (4, 36, new (byte X, byte Y)[] { (190, 121), (193, 121), (197, 121), (190, 124), (194, 124), (198, 124), (190, 128) }),
        (4, 39, new (byte X, byte Y)[] { (232, 120), (236, 120), (240, 120), (232, 124), (236, 124), (240, 124), (232, 128) }),
        (4, 41, new (byte X, byte Y)[] { (120, 230), (124, 230), (128, 230), (120, 234), (124, 234), (128, 234), (120, 238) }),

        // Lost Tower: Devil, rounding out the Lost Tower roster with its own compact spot.
        (4, 37, new (byte X, byte Y)[] { (24, 26), (28, 26), (32, 26), (24, 30), (28, 30), (32, 30), (24, 34) }),

        // Lost Tower: additional Elf Soldier requested for the classic route.
        (4, 257, new (byte X, byte Y)[] { (202, 80) }),

        // Atlans: two compact spots. This map (like the other mid/high-level maps below)
        // already ships with dozens of individually placed monster spawns rather than the
        // huge random-area boxes the early maps relied on, so these are convenience/known
        // farm spots on top of an already decently dense map, not a density fix.
        (7, 45, new (byte X, byte Y)[] { (20, 32), (24, 32), (28, 32), (20, 36), (24, 36), (28, 36), (20, 40) }),
        (7, 46, new (byte X, byte Y)[] { (95, 104), (99, 104), (103, 104), (95, 108), (99, 108), (103, 108), (95, 112) }),

        // Tarkan: two compact spots.
        (8, 61, new (byte X, byte Y)[] { (20, 83), (24, 83), (28, 83), (20, 87), (24, 87), (28, 87), (20, 91) }),
        (8, 58, new (byte X, byte Y)[] { (80, 71), (84, 71), (88, 71), (80, 75), (84, 75), (88, 75), (80, 79) }),

        // Icarus: two compact spots (still requires wings/flying mount to reach).
        (10, 70, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (10, 72, new (byte X, byte Y)[] { (65, 26), (69, 26), (73, 26), (65, 30), (69, 30), (73, 30), (65, 34) }),

        // Aida: two compact spots.
        (33, 307, new (byte X, byte Y)[] { (20, 44), (24, 44), (28, 44), (20, 48), (24, 48), (28, 48), (20, 52) }),
        (33, 308, new (byte X, byte Y)[] { (95, 146), (99, 146), (103, 146), (95, 150), (99, 150), (103, 150), (95, 154) }),

        // Kanturu Ruins: two compact spots.
        (37, 350, new (byte X, byte Y)[] { (35, 68), (39, 68), (43, 68), (35, 72), (39, 72), (43, 72), (35, 76) }),
        (37, 351, new (byte X, byte Y)[] { (95, 65), (99, 65), (103, 65), (95, 69), (99, 69), (103, 69), (95, 73) }),

        // Kanturu Relics: two compact spots.
        (38, 358, new (byte X, byte Y)[] { (110, 134), (114, 134), (118, 134), (110, 138), (114, 138), (118, 138), (110, 142) }),
        (38, 359, new (byte X, byte Y)[] { (185, 137), (189, 137), (193, 137), (185, 141), (189, 141), (193, 141), (185, 145) }),

        // Swamp of Calmness: two compact spots.
        (56, 441, new (byte X, byte Y)[] { (20, 20), (24, 20), (28, 20), (20, 24), (24, 24), (28, 24), (20, 28) }),
        (56, 444, new (byte X, byte Y)[] { (80, 218), (84, 218), (88, 218), (80, 222), (84, 222), (88, 222), (80, 226) }),

        // Raklion: two compact spots (the open field, not the boss room).
        (57, 454, new (byte X, byte Y)[] { (20, 203), (24, 203), (28, 203), (20, 207), (24, 207), (28, 207), (20, 211) }),
        (57, 458, new (byte X, byte Y)[] { (80, 23), (84, 23), (88, 23), (80, 27), (84, 27), (88, 27), (80, 31) }),

        // Kalima 1-7: Aegis and Death Angel, one compact spot each per floor. All seven floors
        // share the same underlying map layout, so the same two coordinates work on every floor.
        (24, 147, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (24, 144, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (25, 177, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (25, 174, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (26, 185, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (26, 182, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (27, 193, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (27, 190, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (28, 263, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (28, 260, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (29, 271, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (29, 268, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
        (36, 331, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (36, 334, new (byte X, byte Y)[] { (80, 77), (84, 77), (88, 77), (80, 81), (84, 81), (88, 81), (80, 85) }),
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
