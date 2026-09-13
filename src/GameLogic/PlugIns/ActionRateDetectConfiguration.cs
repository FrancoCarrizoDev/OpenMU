// <copyright file="ActionRateDetectConfiguration.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.ComponentModel;

/// <summary>
/// Configuration for action-rate and action-timing regularity anti-cheat checks.
/// </summary>
public class ActionRateDetectConfiguration
{
    /// <summary>
    /// Gets or sets the minimum interval between restorative potion consumptions in milliseconds.
    /// The default matches the established half-second potion recovery cooldown.
    /// </summary>
    [DefaultValue(500)]
    public int PotionCooldownMs { get; set; } = 500;

    /// <summary>
    /// Gets or sets the minimum interval between other consumable item requests in milliseconds.
    /// This short interval blocks packet bursts without making ordinary item use feel delayed.
    /// </summary>
    [DefaultValue(250)]
    public int OtherConsumableCooldownMs { get; set; } = 250;

    /// <summary>
    /// Gets or sets a value indicating whether repeated item-cooldown violations automatically ban an account.
    /// </summary>
    [DefaultValue(false)]
    public bool CooldownAutoBan { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether repeated item-cooldown violations disconnect the player.
    /// </summary>
    [DefaultValue(false)]
    public bool CooldownDisconnectOnViolation { get; set; }

    /// <summary>
    /// Gets or sets the number of warnings allowed before an enabled automatic response is applied.
    /// </summary>
    [DefaultValue(3)]
    public int MaxWarnings { get; set; } = 3;

    /// <summary>
    /// Gets or sets the warning debounce period in seconds.
    /// </summary>
    [DefaultValue(5)]
    public int AlertDebounceSeconds { get; set; } = 5;

    /// <summary>
    /// Gets or sets the warning-history expiry period in hours.
    /// </summary>
    [DefaultValue(1)]
    public int WarningHistoryHours { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of most recent intervals retained for an action regularity calculation.
    /// Thirty samples require sustained behavior while keeping memory bounded.
    /// </summary>
    [DefaultValue(30)]
    public int UniformityWindowSize { get; set; } = 30;

    /// <summary>
    /// Gets or sets the minimum number of intervals required before a regularity warning can be raised.
    /// </summary>
    [DefaultValue(24)]
    public int UniformityMinimumSamples { get; set; } = 24;

    /// <summary>
    /// Gets or sets the maximum coefficient of variation (standard deviation divided by mean) which is considered mechanical.
    /// Three percent leaves room for normal client and network jitter; production tuning should be based on observed play.
    /// </summary>
    [DefaultValue(0.03d)]
    public double UniformityCoefficientOfVariationThreshold { get; set; } = 0.03d;

    /// <summary>
    /// Gets or sets a value indicating whether repeated uniformity warnings automatically ban an account.
    /// Defaults to <c>false</c> because highly regular human input can be a false positive.
    /// </summary>
    [DefaultValue(false)]
    public bool UniformityAutoBan { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether repeated uniformity warnings disconnect the player.
    /// Defaults to <c>false</c> until real-game telemetry validates this heuristic.
    /// </summary>
    [DefaultValue(false)]
    public bool UniformityDisconnectOnViolation { get; set; }
}
