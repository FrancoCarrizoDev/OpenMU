// <copyright file="TuneSeasonOneResetPointsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Sets the required level and points granted per reset for Season 1 Classic: reset at level
/// 400 (390 for VIP, enforced separately in <c>ResetCharacterAction</c>), 2000 stat points per
/// reset, multiplied by the reset count so the total points granted accumulate across resets
/// (see ADR-0006).
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("C02D992D-BEE4-44E9-A225-74CA72024CB1")]
public sealed class TuneSeasonOneResetPointsPlugIn : UpdatePlugInBase
{
    private const string PlugInName = "Tune Season 1 Reset Points";
    private const string PlugInDescription = "Sets Season 1 Classic resets to require level 400 and grant 2000 points per reset, accumulating with the reset count.";

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.TuneSeasonOneResetPoints;

    /// <inheritdoc />
    public override string DataInitializationKey => DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 9, 6, 0, 0, 2, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var configuration = gameConfiguration.PlugInConfigurations
            .FirstOrDefault(c => c.TypeId == typeof(ResetFeaturePlugIn).GUID);
        if (configuration is null)
        {
            return ValueTask.CompletedTask;
        }

        if (JsonNode.Parse(configuration.CustomConfiguration ?? "{}") is JsonObject configurationJson)
        {
            configurationJson["RequiredLevel"] = 400;
            configurationJson["PointsPerReset"] = 2000;
            configurationJson["MultiplyPointsByResetCount"] = true;
            configuration.CustomConfiguration = configurationJson.ToJsonString();
        }

        return ValueTask.CompletedTask;
    }
}
