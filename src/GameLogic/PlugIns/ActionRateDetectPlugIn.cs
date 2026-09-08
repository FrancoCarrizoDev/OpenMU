// <copyright file="ActionRateDetectPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MUnique.OpenMU.DataModel.Entities;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Detects client-triggered item bursts and unusually mechanical action timing.
/// </summary>
[PlugIn]
[Display(Name = "Action-rate Anti-Cheat", Description = "Limits item consumption and reports unusually uniform action timing.")]
[Guid("41D26376-ED65-406B-A112-7B70D4C0FA12")]
public class ActionRateDetectPlugIn : IFeaturePlugIn, ISupportCustomConfiguration<ActionRateDetectConfiguration>, ISupportDefaultCustomConfiguration, ISpeedHackCheatCheckPlugIn, IActionRateCheatCheckPlugIn
{
    private readonly ConditionalWeakTable<Player, ActionRateState> _playerStates = new();

    private enum ConsumptionCategory
    {
        Apple,
        HealingPotion,
        ManaPotion,
        ShieldPotion,
        ComplexPotion,
        Other,
    }

    /// <inheritdoc/>
    public ActionRateDetectConfiguration? Configuration { get; set; }

    /// <inheritdoc/>
    public object CreateDefaultConfig() => new ActionRateDetectConfiguration();

    /// <inheritdoc/>
    public ValueTask WalkCheatCheckAsync(Player player, Memory<WalkingStep> steps, SpeedHackCheckEventArgs eventArgs)
    {
        // This plug-in shares the existing attack check point so all attack paths are covered.
        // Walking continues to be handled by SpeedHackDetectPlugIn.
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc/>
    public async ValueTask AttackCheatCheckAsync(Player player, SpeedHackCheckEventArgs eventArgs)
    {
        if (IsServerControlled(player) || this.Configuration is not { } config)
        {
            return;
        }

        var state = this.GetState(player);
        bool isUniform;
        lock (state.Lock)
        {
            var lastAttackTimeMs = state.LastAttackTimeMs;
            isUniform = TrackInterval(
                state.AttackIntervals,
                ref lastAttackTimeMs,
                Environment.TickCount64,
                config);
            state.LastAttackTimeMs = lastAttackTimeMs;
        }

        if (isUniform)
        {
            // Unlike a rate violation, uniform timing is only a suspicion. Do not reject a valid
            // attack or set IsCheatDetected here: the default policy is warning-only.
            await this.RecordViolationAsync(
                player,
                state.UniformityViolations,
                config,
                config.UniformityAutoBan,
                config.UniformityDisconnectOnViolation,
                "unusually uniform action timing",
                "Warning: Unusually uniform action timing detected. This activity is being monitored.").ConfigureAwait(false);
        }
    }

    /// <inheritdoc/>
    public ValueTask ResetMovementStateAsync(Player player) => ValueTask.CompletedTask;

    /// <inheritdoc/>
    public async ValueTask ItemConsumptionCheatCheckAsync(Player player, Item item, ActionRateCheckEventArgs eventArgs)
    {
        if (IsServerControlled(player) || this.Configuration is not { } config)
        {
            return;
        }

        var state = this.GetState(player);
        var now = Environment.TickCount64;
        var category = GetConsumptionCategory(item);
        var cooldownMs = category == ConsumptionCategory.Other ? config.OtherConsumableCooldownMs : config.PotionCooldownMs;
        bool shouldRecordCooldownViolation = false;
        bool isUniform = false;

        lock (state.Lock)
        {
            if (cooldownMs > 0
                && state.LastConsumptionTimesMs.TryGetValue(category, out var lastConsumptionTime)
                && now - lastConsumptionTime < cooldownMs)
            {
                shouldRecordCooldownViolation = true;
            }
            else
            {
                // Reserve the permitted request before awaiting handlers. This makes concurrent packet
                // replays observe the same server-side cooldown and prevents a double consumption.
                state.LastConsumptionTimesMs[category] = now;
                var lastItemConsumptionTimeMs = state.LastItemConsumptionTimeMs;
                isUniform = TrackInterval(
                    state.ItemConsumptionIntervals,
                    ref lastItemConsumptionTimeMs,
                    now,
                    config);
                state.LastItemConsumptionTimeMs = lastItemConsumptionTimeMs;
            }
        }

        if (shouldRecordCooldownViolation)
        {
            eventArgs.IsActionRejected = true;
            await this.RecordViolationAsync(
                player,
                state.CooldownViolations,
                config,
                config.CooldownAutoBan,
                config.CooldownDisconnectOnViolation,
                "item consumption cooldown violation",
                "Warning: Item use rate exceeded. Repeated violations will result in account restriction.").ConfigureAwait(false);
            return;
        }

        if (isUniform)
        {
            await this.RecordViolationAsync(
                player,
                state.UniformityViolations,
                config,
                config.UniformityAutoBan,
                config.UniformityDisconnectOnViolation,
                "unusually uniform item consumption timing",
                "Warning: Unusually uniform action timing detected. This activity is being monitored.").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the warning count from item-cooldown violations for diagnostics and tests.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The current warning count.</returns>
    public int GetCooldownWarningCount(Player player) => this.GetWarningCount(player, static state => state.CooldownViolations);

    /// <summary>
    /// Gets the warning count from timing-uniformity detections for diagnostics and tests.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <returns>The current warning count.</returns>
    public int GetUniformityWarningCount(Player player) => this.GetWarningCount(player, static state => state.UniformityViolations);

    private static bool IsServerControlled(Player player) => player is Offline.OfflinePlayer;

    private static ConsumptionCategory GetConsumptionCategory(Item item)
    {
        var definition = item.Definition;
        if (definition?.Group != 14)
        {
            return ConsumptionCategory.Other;
        }

        // Each restorative potion type recovers a different resource (HP, mana, shield, or a
        // combination of all three) and has always been usable independently of the others: a
        // player - or the MU Helper automating the same recovery loop - routinely drinks an HP
        // potion and a mana potion within the same instant when both drop low together. Sharing
        // one cooldown bucket across every type in this group treated that legitimate pairing as
        // a burst and banned the account after a few such warnings.
        return definition.Number switch
        {
            0 => ConsumptionCategory.Apple,
            >= 1 and <= 3 => ConsumptionCategory.HealingPotion,
            >= 4 and <= 6 => ConsumptionCategory.ManaPotion,
            >= 35 and <= 37 => ConsumptionCategory.ShieldPotion,
            >= 38 and <= 40 => ConsumptionCategory.ComplexPotion,
            _ => ConsumptionCategory.Other,
        };
    }

    private static bool TrackInterval(Queue<long> intervals, ref long lastActionTimeMs, long now, ActionRateDetectConfiguration config)
    {
        bool isUniform = false;
        if (lastActionTimeMs != long.MinValue)
        {
            intervals.Enqueue(Math.Max(0, now - lastActionTimeMs));

            var windowSize = Math.Max(2, config.UniformityWindowSize);
            while (intervals.Count > windowSize)
            {
                intervals.Dequeue();
            }

            var minimumSamples = Math.Clamp(config.UniformityMinimumSamples, 2, windowSize);
            if (intervals.Count >= minimumSamples && HasMechanicalRegularity(intervals, config.UniformityCoefficientOfVariationThreshold))
            {
                // Start a new observation period after a signal. Otherwise a sustained cadence
                // would re-alert as soon as the debounce expires without collecting new evidence.
                intervals.Clear();
                isUniform = true;
            }
        }

        lastActionTimeMs = now;
        return isUniform;
    }

    private static bool HasMechanicalRegularity(IEnumerable<long> intervals, double threshold)
    {
        if (threshold < 0)
        {
            return false;
        }

        var values = intervals.Select(static interval => (double)interval).ToArray();
        var mean = values.Average();
        if (mean <= 0)
        {
            return true;
        }

        var variance = values.Select(value => Math.Pow(value - mean, 2)).Average();
        var coefficientOfVariation = Math.Sqrt(variance) / mean;
        return coefficientOfVariation <= threshold;
    }

    private int GetWarningCount(Player player, Func<ActionRateState, ViolationState> getViolationState)
    {
        var state = this.GetState(player);
        lock (state.Lock)
        {
            return getViolationState(state).AlertTimes.Count;
        }
    }

    private ActionRateState GetState(Player player) => this._playerStates.GetValue(player, static _ => new ActionRateState());

    private async ValueTask RecordViolationAsync(
        Player player,
        ViolationState violationState,
        ActionRateDetectConfiguration config,
        bool autoBan,
        bool disconnectOnViolation,
        string violationDescription,
        string warningMessage)
    {
        var now = DateTime.UtcNow;
        bool shouldBan = false;
        bool shouldWarn = false;
        bool shouldDisconnect = false;

        lock (violationState.Lock)
        {
            if (now - violationState.LastAlertTime < TimeSpan.FromSeconds(config.AlertDebounceSeconds))
            {
                return;
            }

            violationState.LastAlertTime = now;
            violationState.AlertTimes.Enqueue(now);
            while (violationState.AlertTimes.Count > 0 && now - violationState.AlertTimes.Peek() > TimeSpan.FromHours(config.WarningHistoryHours))
            {
                violationState.AlertTimes.Dequeue();
            }

            player.Logger.LogWarning(
                "Action-rate warning issued for player {Player}: {Violation}. Total warnings in the configured history: {WarningCount}",
                player.Name,
                violationDescription,
                violationState.AlertTimes.Count);

            if (violationState.AlertTimes.Count > config.MaxWarnings)
            {
                if (autoBan && player.Account is { } account)
                {
                    player.Logger.LogError("Player {Player} exceeded the action-rate warning limit for {Violation}. Banning account {Account} and disconnecting.", player.Name, violationDescription, account.LoginName);
                    account.State = AccountState.Banned;
                    shouldBan = true;
                }

                if (disconnectOnViolation)
                {
                    shouldDisconnect = true;
                }

                shouldWarn = !shouldBan && !shouldDisconnect;
            }
            else
            {
                shouldWarn = true;
            }
        }

        if (shouldBan)
        {
            await player.SaveProgressAsync().ConfigureAwait(false);
        }

        if (shouldBan || shouldDisconnect)
        {
            await player.DisconnectAsync().ConfigureAwait(false);
        }
        else if (shouldWarn)
        {
            await player.ShowBlueMessageAsync(warningMessage).ConfigureAwait(false);
        }
    }

    private sealed class ActionRateState
    {
        public object Lock { get; } = new();

        public Dictionary<ConsumptionCategory, long> LastConsumptionTimesMs { get; } = new();

        public Queue<long> AttackIntervals { get; } = new();

        public Queue<long> ItemConsumptionIntervals { get; } = new();

        public long LastAttackTimeMs { get; set; } = long.MinValue;

        public long LastItemConsumptionTimeMs { get; set; } = long.MinValue;

        public ViolationState CooldownViolations { get; } = new();

        public ViolationState UniformityViolations { get; } = new();
    }

    private sealed class ViolationState
    {
        public object Lock { get; } = new();

        public DateTime LastAlertTime { get; set; } = DateTime.MinValue;

        public Queue<DateTime> AlertTimes { get; } = new();
    }
}
