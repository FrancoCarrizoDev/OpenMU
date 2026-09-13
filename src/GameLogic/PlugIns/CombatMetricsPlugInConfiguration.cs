// <copyright file="CombatMetricsPlugInConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

/// <summary>
/// Configuration for <see cref="CombatMetricsPlugIn"/>.
/// </summary>
public class CombatMetricsPlugInConfiguration
{
    /// <summary>
    /// Gets or sets how often the in-memory aggregates are persisted.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromMinutes(1);
}
