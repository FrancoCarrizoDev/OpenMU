// <copyright file="AddSeasonOneLostTowerElfSoldierPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Adds the Season 1 Classic Elf Soldier to the requested Lost Tower position.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("D3A0D08B-9E3C-4E7D-9E2B-3BCB86A7B0C8")]
public sealed class AddSeasonOneLostTowerElfSoldierPlugIn : UpdatePlugInBase
{
    private const string PlugInName = "Add Season 1 Lost Tower Elf Soldier";
    private const string PlugInDescription = "Adds an Elf Soldier to Lost Tower at coordinates 202, 80.";

    private const short ElfSoldierNumber = 257;
    private const short SpawnNumber = 698;

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.AddSeasonOneLostTowerElfSoldier;

    /// <inheritdoc />
    public override string DataInitializationKey => DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 9, 6, 0, 0, 2, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var lostTower = gameConfiguration.Maps.FirstOrDefault(map => map.Number == 4 && map.Discriminator == 0);
        var elfSoldier = gameConfiguration.Monsters.FirstOrDefault(monster => monster.Number == ElfSoldierNumber);
        if (lostTower is null || elfSoldier is null
            || lostTower.MonsterSpawns.Any(spawn => spawn.MonsterDefinition?.Number == ElfSoldierNumber
                                                    && spawn.X1 == 202
                                                    && spawn.X2 == 202
                                                    && spawn.Y1 == 80
                                                    && spawn.Y2 == 80))
        {
            return ValueTask.CompletedTask;
        }

        var spawn = context.CreateNew<MonsterSpawnArea>();
        spawn.SetGuid(lostTower.Number, SpawnNumber);
        spawn.GameMap = lostTower;
        spawn.MonsterDefinition = elfSoldier;
        spawn.Quantity = 1;
        spawn.Direction = Direction.SouthEast;
        spawn.SpawnTrigger = SpawnTrigger.Automatic;
        spawn.X1 = 202;
        spawn.X2 = 202;
        spawn.Y1 = 80;
        spawn.Y2 = 80;
        lostTower.MonsterSpawns.Add(spawn);

        return ValueTask.CompletedTask;
    }
}
