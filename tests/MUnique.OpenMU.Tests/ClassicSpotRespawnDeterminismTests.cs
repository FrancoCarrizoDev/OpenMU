// <copyright file="ClassicSpotRespawnDeterminismTests.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Tests;

using Microsoft.Extensions.Logging.Abstractions;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.Persistence.InMemory;

/// <summary>
/// Verifies that the Season 1 <see cref="ClassicSpotsInitializer"/> spots always (re)spawn their
/// monsters at the same fixed origin point, instead of a random position within an area.
/// </summary>
/// <remarks>
/// The expected origin points mirror <see cref="ClassicSpotsInitializer"/>'s own data on purpose: this
/// pins down the exact coordinates the initializer must keep producing, independently of its internal
/// layout, and lets the fixed-point lookups below identify only the classic spots' own spawn areas among
/// a map's other (pre-existing) point spawns for the same monster.
/// </remarks>
[TestFixture]
public class ClassicSpotRespawnDeterminismTests
{
    private static readonly (byte MapNumber, short MonsterNumber, (byte X, byte Y)[] Points)[] ExpectedSpots =
    {
        (0, 2, new (byte X, byte Y)[] { (150, 50), (154, 50), (158, 50), (150, 54), (154, 54), (158, 54), (150, 58) }),
        (0, 3, new (byte X, byte Y)[] { (195, 120), (199, 120), (203, 120), (195, 124), (199, 124), (203, 124), (195, 128) }),
        (0, 0, new (byte X, byte Y)[] { (205, 50), (209, 50), (213, 50), (205, 54), (209, 54), (213, 54), (205, 58) }),
        (0, 1, new (byte X, byte Y)[] { (45, 100), (49, 100), (53, 100), (45, 104), (49, 104), (53, 104), (45, 108) }),
        (0, 4, new (byte X, byte Y)[] { (170, 86), (174, 86), (178, 86), (170, 90), (174, 90), (178, 90), (170, 94) }),
        (0, 6, new (byte X, byte Y)[] { (110, 200), (114, 200), (118, 200), (110, 204), (114, 204), (118, 204), (110, 208) }),
        (0, 7, new (byte X, byte Y)[] { (15, 65), (19, 65), (23, 65), (15, 69), (19, 69), (23, 69), (15, 73) }),

        (3, 26, new (byte X, byte Y)[] { (175, 50), (179, 50), (183, 50), (175, 54), (179, 54), (183, 54), (175, 58) }),
        (3, 27, new (byte X, byte Y)[] { (210, 70), (214, 70), (218, 70), (210, 74), (214, 74), (218, 74), (210, 78) }),
        (3, 28, new (byte X, byte Y)[] { (70, 175), (74, 175), (78, 175), (70, 178), (74, 179), (78, 179), (70, 183) }),
        (3, 29, new (byte X, byte Y)[] { (150, 200), (154, 200), (158, 200), (150, 204), (154, 204), (158, 204), (150, 208) }),
        (3, 30, new (byte X, byte Y)[] { (30, 180), (34, 180), (38, 180), (30, 184), (34, 184), (38, 184), (30, 188) }),
        (3, 31, new (byte X, byte Y)[] { (200, 180), (204, 180), (208, 180), (200, 184), (204, 184), (208, 184), (200, 188) }),

        (2, 21, new (byte X, byte Y)[] { (30, 20), (34, 20), (38, 20), (30, 24), (34, 24), (38, 24), (30, 27) }),
        (2, 22, new (byte X, byte Y)[] { (62, 70), (64, 70), (68, 70), (60, 74), (64, 74), (68, 74), (60, 77) }),
        (2, 19, new (byte X, byte Y)[] { (196, 200), (199, 200), (203, 200), (196, 203), (199, 204), (203, 204), (195, 208) }),
        (2, 20, new (byte X, byte Y)[] { (225, 225), (229, 225), (231, 225), (225, 229), (229, 229), (231, 227), (225, 233) }),
        (2, 23, new (byte X, byte Y)[] { (140, 77), (144, 77), (148, 77), (140, 81), (144, 81), (148, 81), (140, 85) }),
        (2, 24, new (byte X, byte Y)[] { (190, 107), (194, 107), (198, 107), (190, 111), (194, 111), (198, 111), (190, 115) }),

        (1, 8, new (byte X, byte Y)[] { (40, 115), (44, 115), (46, 115), (40, 119), (44, 119), (46, 117), (40, 123) }),
        (1, 14, new (byte X, byte Y)[] { (100, 215), (104, 215), (108, 215), (100, 219), (104, 219), (108, 219), (100, 223) }),
        (1, 11, new (byte X, byte Y)[] { (135, 205), (139, 205), (143, 205), (135, 208), (139, 209), (143, 209), (135, 213) }),
        (1, 17, new (byte X, byte Y)[] { (230, 166), (233, 166), (238, 165), (230, 169), (234, 169), (238, 169), (230, 172) }),
        (1, 18, new (byte X, byte Y)[] { (20, 80), (24, 80), (28, 80), (20, 84), (24, 84), (28, 84), (20, 88) }),

        (4, 40, new (byte X, byte Y)[] { (5, 95), (9, 95), (13, 95), (5, 99), (9, 99), (13, 99), (5, 103) }),
        (4, 36, new (byte X, byte Y)[] { (190, 121), (193, 121), (197, 121), (190, 124), (194, 124), (198, 124), (190, 128) }),
        (4, 39, new (byte X, byte Y)[] { (232, 120), (236, 120), (240, 120), (232, 124), (236, 124), (240, 124), (232, 128) }),
        (4, 41, new (byte X, byte Y)[] { (120, 230), (124, 230), (128, 230), (120, 234), (124, 234), (128, 234), (120, 238) }),
        (4, 37, new (byte X, byte Y)[] { (24, 26), (28, 26), (32, 26), (24, 30), (28, 30), (32, 30), (24, 34) }),

        (7, 45, new (byte X, byte Y)[] { (20, 32), (24, 32), (28, 32), (20, 36), (24, 36), (28, 36), (20, 40) }),
        (7, 46, new (byte X, byte Y)[] { (95, 104), (99, 104), (103, 104), (95, 108), (99, 108), (103, 108), (95, 112) }),

        (8, 61, new (byte X, byte Y)[] { (20, 83), (24, 83), (28, 83), (20, 87), (24, 87), (28, 87), (20, 91) }),
        (8, 58, new (byte X, byte Y)[] { (80, 71), (84, 71), (88, 71), (80, 75), (84, 75), (88, 75), (80, 79) }),

        (10, 70, new (byte X, byte Y)[] { (20, 23), (24, 23), (28, 23), (20, 27), (24, 27), (28, 27), (20, 31) }),
        (10, 72, new (byte X, byte Y)[] { (65, 26), (69, 26), (73, 26), (65, 30), (69, 30), (73, 30), (65, 34) }),

        (33, 307, new (byte X, byte Y)[] { (20, 44), (24, 44), (28, 44), (20, 48), (24, 48), (28, 48), (20, 52) }),
        (33, 308, new (byte X, byte Y)[] { (95, 146), (99, 146), (103, 146), (95, 150), (99, 150), (103, 150), (95, 154) }),

        (37, 350, new (byte X, byte Y)[] { (35, 68), (39, 68), (43, 68), (35, 72), (39, 72), (43, 72), (35, 76) }),
        (37, 351, new (byte X, byte Y)[] { (95, 65), (99, 65), (103, 65), (95, 69), (99, 69), (103, 69), (95, 73) }),

        (38, 358, new (byte X, byte Y)[] { (110, 134), (114, 134), (118, 134), (110, 138), (114, 138), (118, 138), (110, 142) }),
        (38, 359, new (byte X, byte Y)[] { (185, 137), (189, 137), (193, 137), (185, 141), (189, 141), (193, 141), (185, 145) }),

        (56, 441, new (byte X, byte Y)[] { (20, 20), (24, 20), (28, 20), (20, 24), (24, 24), (28, 24), (20, 28) }),
        (56, 444, new (byte X, byte Y)[] { (80, 218), (84, 218), (88, 218), (80, 222), (84, 222), (88, 222), (80, 226) }),

        (57, 454, new (byte X, byte Y)[] { (20, 203), (24, 203), (28, 203), (20, 207), (24, 207), (28, 207), (20, 211) }),
        (57, 458, new (byte X, byte Y)[] { (80, 23), (84, 23), (88, 23), (80, 27), (84, 27), (88, 27), (80, 31) }),

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

    private GameConfiguration _gameConfiguration = null!;

    /// <summary>
    /// Runs the real Season 1 data initialization, including real map terrain, once for all tests.
    /// </summary>
    [OneTimeSetUp]
    public async Task SetUpAsync()
    {
        var contextProvider = new InMemoryPersistenceContextProvider();
        var dataInitialization = new DataInitialization(contextProvider, new NullLoggerFactory());
        await dataInitialization.CreateInitialDataAsync(1, true).ConfigureAwait(false);

        using var context = contextProvider.CreateNewConfigurationContext();
        this._gameConfiguration = (await context.GetAsync<GameConfiguration>().ConfigureAwait(false)).Single();
    }

    /// <summary>
    /// Each classic spot must contain exactly one fixed-point (<see cref="MonsterSpawnArea.IsPoint"/>)
    /// spawn area of quantity 1 per expected origin, preserving the spot's original total monster count.
    /// </summary>
    [TestCaseSource(nameof(ExpectedSpots))]
    public void ClassicSpotIsRepresentedAsFixedPoints((byte MapNumber, short MonsterNumber, (byte X, byte Y)[] Points) spot)
    {
        var map = this._gameConfiguration.Maps.Single(m => m.Number == spot.MapNumber && m.Discriminator == 0);

        foreach (var point in spot.Points)
        {
            var matches = map.MonsterSpawns
                .Where(s => s.MonsterDefinition?.Number == spot.MonsterNumber
                            && s.SpawnTrigger == SpawnTrigger.Automatic
                            && s.IsPoint()
                            && s.X1 == point.X && s.Y1 == point.Y)
                .ToList();

            Assert.That(matches, Has.Count.EqualTo(1), $"Expected exactly one fixed-point spawn at ({point.X}, {point.Y}) for monster {spot.MonsterNumber} on map {spot.MapNumber}.");
            Assert.That(matches[0].Quantity, Is.EqualTo((short)1));
        }
    }

    /// <summary>
    /// Simulates repeated deaths/respawns of every classic spot monster (via
    /// <see cref="NonPlayerCharacter.Initialize"/>, the same method the game calls on respawn) and
    /// verifies each one always comes back at the exact same coordinate, on the real map terrain.
    /// </summary>
    [TestCaseSource(nameof(ExpectedSpots))]
    public void ClassicSpotMonstersAlwaysRespawnAtTheirFixedOrigin((byte MapNumber, short MonsterNumber, (byte X, byte Y)[] Points) spot)
    {
        var mapDefinition = this._gameConfiguration.Maps.Single(m => m.Number == spot.MapNumber && m.Discriminator == 0);
        var gameMap = new GameMap(mapDefinition, TimeSpan.Zero, 8);

        foreach (var point in spot.Points)
        {
            var spawnArea = mapDefinition.MonsterSpawns.Single(
                s => s.MonsterDefinition?.Number == spot.MonsterNumber
                     && s.SpawnTrigger == SpawnTrigger.Automatic
                     && s.IsPoint()
                     && s.X1 == point.X && s.Y1 == point.Y);

            var npc = new NonPlayerCharacter(spawnArea, spawnArea.MonsterDefinition!, gameMap);
            var expectedPosition = new MUnique.OpenMU.Pathfinding.Point(point.X, point.Y);

            for (var respawn = 0; respawn < 10; respawn++)
            {
                npc.Initialize();
                Assert.That(npc.Position, Is.EqualTo(expectedPosition), $"Respawn #{respawn} moved the monster away from its fixed origin.");
            }
        }
    }
}

