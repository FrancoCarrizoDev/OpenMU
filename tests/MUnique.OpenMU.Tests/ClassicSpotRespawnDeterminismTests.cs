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

        (3, 26, new (byte X, byte Y)[] { (175, 50), (179, 50), (183, 50), (175, 54), (179, 54), (183, 54), (175, 58) }),
        (3, 27, new (byte X, byte Y)[] { (210, 70), (214, 70), (218, 70), (210, 74), (214, 74), (218, 74), (210, 78) }),
        (3, 28, new (byte X, byte Y)[] { (70, 175), (74, 175), (78, 175), (70, 178), (74, 179), (78, 179), (70, 183) }),
        (3, 29, new (byte X, byte Y)[] { (150, 200), (154, 200), (158, 200), (150, 204), (154, 204), (158, 204), (150, 208) }),

        (2, 21, new (byte X, byte Y)[] { (30, 20), (34, 20), (38, 20), (30, 24), (34, 24), (38, 24), (30, 27) }),
        (2, 22, new (byte X, byte Y)[] { (62, 70), (64, 70), (68, 70), (60, 74), (64, 74), (68, 74), (60, 77) }),
        (2, 19, new (byte X, byte Y)[] { (196, 200), (199, 200), (203, 200), (196, 203), (199, 204), (203, 204), (195, 208) }),
        (2, 20, new (byte X, byte Y)[] { (225, 225), (229, 225), (231, 225), (225, 229), (229, 229), (231, 227), (225, 233) }),

        (1, 8, new (byte X, byte Y)[] { (40, 115), (44, 115), (46, 115), (40, 119), (44, 119), (46, 117), (40, 123) }),
        (1, 14, new (byte X, byte Y)[] { (100, 215), (104, 215), (108, 215), (100, 219), (104, 219), (108, 219), (100, 223) }),
        (1, 11, new (byte X, byte Y)[] { (135, 205), (139, 205), (143, 205), (135, 208), (139, 209), (143, 209), (135, 213) }),
        (1, 17, new (byte X, byte Y)[] { (230, 166), (233, 166), (238, 165), (230, 169), (234, 169), (238, 169), (230, 172) }),

        (4, 40, new (byte X, byte Y)[] { (5, 95), (9, 95), (13, 95), (5, 99), (9, 99), (13, 99), (5, 103) }),
        (4, 36, new (byte X, byte Y)[] { (190, 121), (193, 121), (197, 121), (190, 124), (194, 124), (198, 124), (190, 128) }),
        (4, 39, new (byte X, byte Y)[] { (232, 120), (236, 120), (240, 120), (232, 124), (236, 124), (240, 124), (232, 128) }),
        (4, 41, new (byte X, byte Y)[] { (120, 230), (124, 230), (128, 230), (120, 234), (124, 234), (128, 234), (120, 238) }),
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

