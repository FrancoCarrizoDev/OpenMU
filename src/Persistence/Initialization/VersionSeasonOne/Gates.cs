// <copyright file="Gates.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;

using MUnique.OpenMU.DataModel.Configuration;

/// <summary>
/// Gates initialization for the classic "Season 1" style server.
///
/// This is <see cref="VersionSeasonSix.Gates"/> with every gate/warp entry removed that points at a
/// map excluded by <see cref="GameMapsInitializer"/> (see the remarks there for which maps and why).
/// It can't be a subclass that overrides just the excluded parts: all of season6's gate-building
/// methods are private and one another's blocks reference each other's target gates by number
/// (e.g. Valley of Loren - a map we keep - has an entrance gate whose *target* is physically on
/// Crywolf Fortress - a map we exclude), so this is a full copy with the affected lines removed
/// rather than a diff-friendly override.
///
/// Also drops <c>CreateDuelConfiguration</c> entirely: Duel Arena's only entrance is via Vulcanus,
/// both excluded. Leaving <see cref="GameConfiguration.DuelConfiguration"/> unset is safe and not
/// season1-specific - Version075 and Version095d already never set it either.
/// </summary>
public class Gates : InitializerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Gates" /> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public Gates(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <summary>
    /// Initializes the gates.
    /// </summary>
    public override void Initialize()
    {
        var maps = this.GameConfiguration.Maps.ToDictionary(map => new MapIdentity(map.Number, map.Discriminator), map => map);
        var targetGates = this.CreateTargetGates(maps);
        this.CreateEnterGates(maps, targetGates);
        this.CreateWarpEntries(targetGates);
    }

    /// <summary>
    /// Creates the warp entries.
    /// </summary>
    /// <param name="gates">The gates.</param>
    private void CreateWarpEntries(IDictionary<short, ExitGate> gates)
    {
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(1, "Arena", 2000, 50, gates[50]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(2, "Lorencia", 2000, 10, gates[17]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(3, "Noria", 2000, 10, gates[27]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(4, "Devias", 2000, 20, gates[22]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(5, "Devias2", 2500, 20, gates[72]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(6, "Devias3", 3000, 20, gates[73]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(7, "Devias4", 3500, 20, gates[74]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(8, "Dungeon", 3000, 30, gates[2]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(9, "Dungeon2", 3500, 40, gates[6]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(10, "Dungeon3", 4000, 50, gates[10]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(11, "Atlans", 4000, 70, gates[49]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(12, "Atlans2", 4500, 80, gates[75]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(13, "Atlans3", 5000, 90, gates[76]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(14, "LostTower", 5000, 50, gates[42]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(15, "LostTower2", 5500, 50, gates[31]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(16, "LostTower3", 6000, 50, gates[33]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(17, "LostTower4", 6500, 60, gates[35]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(18, "LostTower5", 7000, 60, gates[37]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(19, "LostTower6", 7500, 70, gates[39]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(20, "LostTower7", 8000, 70, gates[41]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(21, "Tarkan", 8000, 140, gates[57]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(22, "Tarkan2", 8500, 140, gates[77]));
        this.GameConfiguration.WarpList.Add(this.CreateWarpInfo(23, "Icarus", 10000, 170, gates[63]));
    }

    private WarpInfo CreateWarpInfo(ushort index, string name, int costs, int levelRequirement, ExitGate gate)
    {
        var warpInfo = this.Context.CreateNew<WarpInfo>();
        warpInfo.Index = index;
        warpInfo.Name = name;
        warpInfo.Costs = costs;
        warpInfo.LevelRequirement = levelRequirement;
        warpInfo.Gate = gate;
        return warpInfo;
    }

    private ExitGate CreateExitGate(GameMapDefinition map, byte x1, byte y1, byte x2, byte y2, byte direction, bool isSpawnGate = false)
    {
        if (x1 > x2)
        {
            throw new ArgumentException("x1 > x2");
        }

        if (y1 > y2)
        {
            throw new ArgumentException("y1 > y2");
        }

        var gate = this.Context.CreateNew<ExitGate>();
        gate.Map = map;
        gate.X1 = x1;
        gate.Y1 = y1;
        gate.X2 = x2;
        gate.Y2 = y2;

        // different to all other configurations, 0 means 'Undefined', so we just assume that we can cast it to Direction without adding 1.
        gate.Direction = (Direction)direction;
        gate.IsSpawnGate = isSpawnGate;
        map.ExitGates.Add(gate);
        return gate;
    }

    /// <summary>
    /// Creates the target gates and adds them to the <see cref="GameMapDefinition.ExitGates" /> if the gate is specified as spawn gate (flag == 0).
    /// </summary>
    /// <param name="maps">The previously created game maps.</param>
    /// <returns>A dictionary of all created exit gates. The key is the number.</returns>
    private IDictionary<short, ExitGate> CreateTargetGates(IDictionary<MapIdentity, GameMapDefinition> maps)
    {
        var targetGates = new Dictionary<short, ExitGate>();

        // Lorencia
        targetGates.Add(17, this.CreateExitGate(maps[0], 133, 118, 151, 135, 0, true));
        targetGates.Add(4, this.CreateExitGate(maps[0], 121, 231, 123, 231, 1));
        targetGates.Add(21, this.CreateExitGate(maps[0], 7, 38, 8, 41, 3));
        targetGates.Add(26, this.CreateExitGate(maps[0], 213, 244, 217, 245, 1));
        targetGates.Add(108, this.CreateExitGate(maps[0], 235, 13, 239, 13, 0));

        // Dungeon
        targetGates.Add(2, this.CreateExitGate(maps[1], 107, 247, 110, 247, 1));
        targetGates.Add(6, this.CreateExitGate(maps[1], 231, 126, 234, 127, 1));
        targetGates.Add(8, this.CreateExitGate(maps[1], 240, 149, 241, 151, 3));
        targetGates.Add(10, this.CreateExitGate(maps[1], 3, 83, 4, 86, 3));
        targetGates.Add(12, this.CreateExitGate(maps[1], 3, 16, 6, 17, 3));
        targetGates.Add(14, this.CreateExitGate(maps[1], 29, 125, 30, 126, 1));
        targetGates.Add(16, this.CreateExitGate(maps[1], 5, 32, 7, 33, 1));

        // Devias
        targetGates.Add(22, this.CreateExitGate(maps[2], 197, 35, 218, 50, 0, true));
        targetGates.Add(72, this.CreateExitGate(maps[2], 23, 24, 27, 27, 0));
        targetGates.Add(73, this.CreateExitGate(maps[2], 224, 227, 227, 231, 0));
        targetGates.Add(74, this.CreateExitGate(maps[2], 69, 178, 72, 181, 0));
        targetGates.Add(289, this.CreateExitGate(maps[2], 52, 88, 54, 89, 0));
        targetGates.Add(19, this.CreateExitGate(maps[2], 242, 34, 243, 37, 7));
        targetGates.Add(44, this.CreateExitGate(maps[2], 2, 246, 3, 247, 2));
        targetGates.Add(262, this.CreateExitGate(maps[2], 161, 241, 163, 242, 0));

        // Noria
        targetGates.Add(27, this.CreateExitGate(maps[3], 171, 108, 177, 117, 0, true));
        targetGates.Add(24, this.CreateExitGate(maps[3], 148, 5, 155, 6, 5));
        targetGates.Add(48, this.CreateExitGate(maps[3], 240, 240, 241, 243, 7));
        targetGates.Add(122, this.CreateExitGate(maps[3], 220, 31, 226, 34, 0));

        // Lost Tower
        targetGates.Add(42, this.CreateExitGate(maps[4], 203, 70, 213, 81, 0, true));
        targetGates.Add(29, this.CreateExitGate(maps[4], 162, 2, 166, 3, 5));
        targetGates.Add(31, this.CreateExitGate(maps[4], 241, 237, 244, 238, 1));
        targetGates.Add(33, this.CreateExitGate(maps[4], 86, 166, 87, 168, 3));
        targetGates.Add(35, this.CreateExitGate(maps[4], 87, 86, 88, 89, 3));
        targetGates.Add(37, this.CreateExitGate(maps[4], 128, 53, 131, 54, 1));
        targetGates.Add(39, this.CreateExitGate(maps[4], 52, 53, 55, 54, 1));
        targetGates.Add(41, this.CreateExitGate(maps[4], 8, 85, 9, 87, 1));
        targetGates.Add(65, this.CreateExitGate(maps[4], 17, 249, 19, 249, 1));

        // Exile
        // Arena
        targetGates.Add(50, this.CreateExitGate(maps[6], 101, 115, 103, 117, 0, true));
        targetGates.Add(51, this.CreateExitGate(maps[6], 107, 115, 107, 115, 0, true));
        targetGates.Add(52, this.CreateExitGate(maps[6], 107, 114, 107, 114, 0, true));

        // Atlans
        targetGates.Add(49, this.CreateExitGate(maps[7], 15, 11, 27, 23, 0, true));
        targetGates.Add(75, this.CreateExitGate(maps[7], 225, 50, 228, 53, 0));
        targetGates.Add(76, this.CreateExitGate(maps[7], 62, 157, 68, 163, 0));
        targetGates.Add(46, this.CreateExitGate(maps[7], 14, 12, 15, 13, 3));
        targetGates.Add(56, this.CreateExitGate(maps[7], 16, 225, 17, 230, 3));
        targetGates.Add(266, this.CreateExitGate(maps[7], 16, 19, 17, 20, 0));

        // Tarkan
        targetGates.Add(57, this.CreateExitGate(maps[8], 187, 63, 203, 69, 0, true));
        targetGates.Add(77, this.CreateExitGate(maps[8], 91, 160, 93, 161, 0));
        targetGates.Add(54, this.CreateExitGate(maps[8], 248, 40, 251, 44, 7));
        targetGates.Add(128, this.CreateExitGate(maps[8], 7, 199, 7, 201, 0));

        // Devil Square
        targetGates.Add(58, this.CreateExitGate(maps[new(9, 1)], 133, 91, 141, 99, 0));
        targetGates.Add(59, this.CreateExitGate(maps[new(9, 2)], 135, 162, 142, 170, 0));
        targetGates.Add(60, this.CreateExitGate(maps[new(9, 3)], 62, 150, 70, 158, 0));
        targetGates.Add(61, this.CreateExitGate(maps[new(9, 4)], 66, 84, 74, 92, 0));

        // Icarus
        targetGates.Add(63, this.CreateExitGate(maps[10], 14, 13, 16, 13, 5));

        // Blood Castle
        targetGates.Add(66, this.CreateExitGate(maps[11], 12, 5, 14, 10, 0));
        targetGates.Add(67, this.CreateExitGate(maps[12], 12, 5, 14, 10, 0));
        targetGates.Add(68, this.CreateExitGate(maps[13], 12, 5, 14, 10, 0));
        targetGates.Add(69, this.CreateExitGate(maps[14], 12, 5, 14, 10, 0));
        targetGates.Add(70, this.CreateExitGate(maps[15], 12, 5, 14, 10, 0));
        targetGates.Add(71, this.CreateExitGate(maps[16], 12, 5, 14, 10, 0));
        targetGates.Add(80, this.CreateExitGate(maps[17], 12, 5, 14, 10, 0));
        targetGates.Add(271, this.CreateExitGate(maps[52], 12, 5, 14, 10, 0));

        // Chaos Castle
        targetGates.Add(82, this.CreateExitGate(maps[18], 31, 88, 36, 95, 0));
        targetGates.Add(83, this.CreateExitGate(maps[19], 31, 88, 36, 95, 0));
        targetGates.Add(84, this.CreateExitGate(maps[20], 31, 88, 36, 95, 0));
        targetGates.Add(85, this.CreateExitGate(maps[21], 31, 88, 36, 95, 0));
        targetGates.Add(86, this.CreateExitGate(maps[22], 31, 88, 36, 95, 0));
        targetGates.Add(87, this.CreateExitGate(maps[23], 31, 88, 36, 95, 0));

        // Kalima
        targetGates.Add(88, this.CreateExitGate(maps[24], 10, 16, 17, 22, 0));
        targetGates.Add(89, this.CreateExitGate(maps[25], 10, 16, 17, 22, 0));
        targetGates.Add(90, this.CreateExitGate(maps[26], 10, 16, 17, 22, 0));
        targetGates.Add(91, this.CreateExitGate(maps[27], 10, 16, 17, 22, 0));
        targetGates.Add(92, this.CreateExitGate(maps[28], 10, 16, 17, 22, 0));
        targetGates.Add(93, this.CreateExitGate(maps[29], 10, 16, 17, 22, 0));
        targetGates.Add(116, this.CreateExitGate(maps[36], 10, 16, 17, 22, 0));

        // Valley of Loren
        targetGates.Add(94, this.CreateExitGate(maps[30], 88, 31, 102, 46, 0, true));
        targetGates.Add(100, this.CreateExitGate(maps[30], 39, 14, 142, 50, 0, true));
        targetGates.Add(101, this.CreateExitGate(maps[30], 84, 180, 100, 222, 0, true));
        targetGates.Add(104, this.CreateExitGate(maps[30], 87, 209, 100, 232, 0, true));
        targetGates.Add(105, this.CreateExitGate(maps[30], 72, 10, 104, 199, 0, true));
        targetGates.Add(106, this.CreateExitGate(maps[30], 131, 92, 138, 94, 0, true));
        targetGates.Add(97, this.CreateExitGate(maps[30], 164, 198, 187, 209, 0));
        targetGates.Add(99, this.CreateExitGate(maps[30], 90, 236, 99, 239, 0));
        targetGates.Add(103, this.CreateExitGate(maps[30], 29, 37, 30, 42, 0));
        targetGates.Add(110, this.CreateExitGate(maps[30], 131, 92, 138, 94, 0));
        targetGates.Add(124, this.CreateExitGate(maps[30], 155, 37, 158, 43, 0));

        // Land of Trials
        targetGates.Add(95, this.CreateExitGate(maps[31], 60, 10, 69, 19, 0));

        return targetGates;
    }

    /// <summary>
    /// Creates the enter gates for all maps.
    /// </summary>
    /// <param name="maps">The maps.</param>
    /// <param name="targetGates">The target gates by number.</param>
    private void CreateEnterGates(IDictionary<MapIdentity, GameMapDefinition> maps, IDictionary<short, ExitGate> targetGates)
    {
        maps[0].EnterGates.Add(this.CreateEnterGate(1, targetGates[2], 121, 232, 123, 233, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(3, targetGates[4], 108, 248, 109, 248, 0));
        maps[1].EnterGates.Add(this.CreateEnterGate(5, targetGates[6], 239, 149, 239, 150, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(7, targetGates[8], 232, 127, 233, 128, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(9, targetGates[10], 2, 17, 2, 18, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(11, targetGates[12], 2, 84, 2, 85, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(13, targetGates[14], 5, 34, 6, 34, 20));
        maps[1].EnterGates.Add(this.CreateEnterGate(15, targetGates[16], 29, 127, 30, 127, 20));
        maps[0].EnterGates.Add(this.CreateEnterGate(18, targetGates[19], 5, 38, 6, 41, 15));
        maps[2].EnterGates.Add(this.CreateEnterGate(20, targetGates[21], 244, 34, 245, 37, 0));
        maps[0].EnterGates.Add(this.CreateEnterGate(23, targetGates[24], 213, 246, 217, 247, 10));
        maps[3].EnterGates.Add(this.CreateEnterGate(25, targetGates[26], 148, 3, 155, 4, 10));
        maps[2].EnterGates.Add(this.CreateEnterGate(28, targetGates[29], 2, 248, 3, 249, 40));
        maps[4].EnterGates.Add(this.CreateEnterGate(30, targetGates[31], 190, 6, 191, 8, 40));
        maps[4].EnterGates.Add(this.CreateEnterGate(32, targetGates[33], 166, 163, 167, 166, 40));
        maps[4].EnterGates.Add(this.CreateEnterGate(34, targetGates[35], 132, 245, 135, 246, 50));
        maps[4].EnterGates.Add(this.CreateEnterGate(36, targetGates[37], 132, 135, 135, 136, 50));
        maps[4].EnterGates.Add(this.CreateEnterGate(38, targetGates[39], 131, 15, 132, 18, 50));
        maps[4].EnterGates.Add(this.CreateEnterGate(40, targetGates[41], 6, 5, 7, 8, 50));
        maps[4].EnterGates.Add(this.CreateEnterGate(43, targetGates[44], 162, 0, 166, 1, 15));
        maps[3].EnterGates.Add(this.CreateEnterGate(45, targetGates[46], 242, 240, 245, 243, 60));
        maps[7].EnterGates.Add(this.CreateEnterGate(47, targetGates[48], 9, 9, 11, 12, 60));
        maps[7].EnterGates.Add(this.CreateEnterGate(53, targetGates[54], 14, 225, 15, 230, 130));
        maps[8].EnterGates.Add(this.CreateEnterGate(55, targetGates[56], 246, 40, 247, 44, 130));
        maps[4].EnterGates.Add(this.CreateEnterGate(62, targetGates[63], 17, 250, 19, 250, 160));
        maps[10].EnterGates.Add(this.CreateEnterGate(64, targetGates[65], 14, 12, 16, 12, 50));
        maps[30].EnterGates.Add(this.CreateEnterGate(96, targetGates[97], 93, 242, 95, 243, 0));
        maps[30].EnterGates.Add(this.CreateEnterGate(98, targetGates[99], 160, 203, 161, 205, 0));
        maps[0].EnterGates.Add(this.CreateEnterGate(102, targetGates[103], 239, 14, 240, 15, 10));
        maps[30].EnterGates.Add(this.CreateEnterGate(107, targetGates[108], 28, 40, 28, 41, 0));
        maps[31].EnterGates.Add(this.CreateEnterGate(109, targetGates[110], 59, 7, 63, 8, 0));
    }

    private EnterGate CreateEnterGate(short number, ExitGate targetGate, byte x1, byte y1, byte x2, byte y2, short levelRequirement)
    {
        var enterGate = this.Context.CreateNew<EnterGate>();
        enterGate.Number = number;
        enterGate.LevelRequirement = levelRequirement;
        enterGate.TargetGate = targetGate;
        enterGate.X1 = x1;
        enterGate.Y1 = y1;
        enterGate.X2 = x2;
        enterGate.Y2 = y2;
        enterGate.LevelRequirement = levelRequirement;
        return enterGate;
    }
}
