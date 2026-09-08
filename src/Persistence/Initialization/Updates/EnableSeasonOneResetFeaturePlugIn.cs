// <copyright file="EnableSeasonOneResetFeaturePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using System.Text.Json.Nodes;
using MUnique.OpenMU.GameLogic.Resets;
using MUnique.OpenMU.Persistence.Initialization.VersionSeasonOne;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Enables the reset feature for the Season 1 Classic configuration without imposing a reset limit.
/// </summary>
[PlugIn]
[Display(Name = PlugInName, Description = PlugInDescription)]
[Guid("C3A7E5D4-0E9B-4F0E-8F9E-0AB7F4C9E3A1")]
public sealed class EnableSeasonOneResetFeaturePlugIn : UpdatePlugInBase
{
    private const string PlugInName = "Enable Season 1 Reset Feature";
    private const string PlugInDescription = "Enables unlimited resets for Season 1 Classic.";

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.EnableSeasonOneResetFeature;

    /// <inheritdoc />
    public override string DataInitializationKey => DataInitialization.Id;

    /// <inheritdoc />
    public override bool IsMandatory => true;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2026, 9, 6, 0, 0, 1, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        var configuration = gameConfiguration.PlugInConfigurations
            .FirstOrDefault(c => c.TypeId == typeof(ResetFeaturePlugIn).GUID);
        if (configuration is null)
        {
            return ValueTask.CompletedTask;
        }

        configuration.IsActive = true;

        if (!string.IsNullOrWhiteSpace(configuration.CustomConfiguration))
        {
            if (JsonNode.Parse(configuration.CustomConfiguration) is JsonObject configurationJson)
            {
                configurationJson["ResetLimit"] = null;
                configuration.CustomConfiguration = configurationJson.ToJsonString();
            }
        }

        return ValueTask.CompletedTask;
    }
}
