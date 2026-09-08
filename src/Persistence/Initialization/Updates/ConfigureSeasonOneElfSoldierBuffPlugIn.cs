// <copyright file="ConfigureSeasonOneElfSoldierBuffPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Skills;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Applies the Season 1 Classic duration and reset-based access policy to the Elf Soldier buff.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("0C6C8B31-3B02-4B56-9F4C-5C1A9F3E47D2")]
public sealed class ConfigureSeasonOneElfSoldierBuffPlugIn : UpdatePlugInBase
{
    private const string PlugInName = "Configure Season 1 Elf Soldier Buff";
    private const string PlugInDescription = "Sets the Season 1 Elf Soldier buff to 24 hours and removes its level cap.";

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.ConfigureSeasonOneElfSoldierBuff;

    /// <inheritdoc />
    public override string DataInitializationKey => DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var elfSoldier = gameConfiguration.Monsters.FirstOrDefault(m => m.Number == 257);
        var effect = gameConfiguration.MagicEffects.FirstOrDefault(e => e.Number == (short)MagicEffectNumber.ElfSoldierBuff);
        if (elfSoldier is null || effect is null)
        {
            return ValueTask.CompletedTask;
        }

        effect.Duration ??= context.CreateNew<PowerUpDefinitionValue>();
        effect.Duration.ConstantValue.Value = 24 * 60 * 60;
        effect.SendDuration = true;

        foreach (var buff in elfSoldier.Buffs.Where(b => b.MagicEffectDefinition?.Number == effect.Number))
        {
            buff.MaximumLevel = null;
        }

        return ValueTask.CompletedTask;
    }
}
