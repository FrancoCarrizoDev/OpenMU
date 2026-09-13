// <copyright file="CombatMetricsPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using MUnique.OpenMU.DataModel.Statistics;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Aggregates player damage in memory and periodically persists compact PvE and PvP metrics.
/// </summary>
[PlugIn]
[Display(Name = "Combat metrics", Description = "Records aggregated player damage for PvE and PvP balance analysis.")]
[Guid("9B2D72C0-1AE8-42D7-A36A-A6B4CB238F7D")]
public class CombatMetricsPlugIn : IAttackableGotHitPlugIn, IPeriodicTaskPlugIn, ISupportCustomConfiguration<CombatMetricsPlugInConfiguration>, ISupportDefaultCustomConfiguration
{
    private ConcurrentDictionary<CombatMetricKey, CombatMetricAccumulator> _metrics = new();
    private DateTime _intervalStartUtc = DateTime.UtcNow;
    private DateTime _nextFlushUtc = DateTime.UtcNow.AddMinutes(1);

    /// <inheritdoc />
    public CombatMetricsPlugInConfiguration? Configuration { get; set; }

    /// <inheritdoc />
    public void AttackableGotHit(IAttackable attackable, IAttacker attacker, HitInfo hitInfo, Skill? skill)
    {
        if (hitInfo is { HealthDamage: 0, ShieldDamage: 0 })
        {
            return;
        }

        if (!TryGetAttackingPlayer(attacker, out var player) || player.SelectedCharacter is null || player.Account?.IsBot == true)
        {
            return;
        }

        var combatType = attackable is Player ? CombatType.PlayerVersusPlayer : CombatType.PlayerVersusEnvironment;
        var mapNumber = (short)(player.CurrentMap?.Definition.Number ?? 0);
        var skillNumber = skill?.Number ?? 0;
        var key = new CombatMetricKey(player.SelectedCharacter.Id, combatType, mapNumber, skillNumber);
        this._metrics.GetOrAdd(key, static _ => new CombatMetricAccumulator()).Add(hitInfo);
    }

    /// <inheritdoc />
    public async ValueTask ExecuteTaskAsync(GameContext gameContext)
    {
        if (DateTime.UtcNow < this._nextFlushUtc)
        {
            return;
        }

        var configuration = this.Configuration ??= CreateDefaultConfiguration();
        var metrics = Interlocked.Exchange(ref this._metrics, new ConcurrentDictionary<CombatMetricKey, CombatMetricAccumulator>());
        var intervalStartUtc = this._intervalStartUtc;
        this._intervalStartUtc = DateTime.UtcNow;
        this._nextFlushUtc = this._intervalStartUtc + configuration.FlushInterval;

        await this.SaveMetricsAsync(gameContext, intervalStartUtc, metrics).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void ForceStart()
    {
        this._nextFlushUtc = DateTime.UtcNow;
    }

    /// <inheritdoc />
    public object CreateDefaultConfig()
    {
        return CreateDefaultConfiguration();
    }

    private static bool TryGetAttackingPlayer(IAttacker attacker, out Player player)
    {
        player = attacker as Player ?? (attacker as Monster)?.SummonedBy!;
        return player is not null;
    }

    private static CombatMetricsPlugInConfiguration CreateDefaultConfiguration()
    {
        return new CombatMetricsPlugInConfiguration();
    }

    private async ValueTask SaveMetricsAsync(GameContext gameContext, DateTime intervalStartUtc, ConcurrentDictionary<CombatMetricKey, CombatMetricAccumulator> metrics)
    {
        if (metrics.IsEmpty)
        {
            return;
        }

        try
        {
            using var context = gameContext.PersistenceContextProvider.CreateNewTypedContext(typeof(CombatMetric), false, gameContext.Configuration);
            var gameServerId = (gameContext as IGameServerContext)?.Id ?? 0;
            foreach (var (key, accumulator) in metrics)
            {
                var snapshot = accumulator.GetSnapshot();
                if (snapshot.HitCount == 0)
                {
                    continue;
                }

                var metric = context.CreateNew<CombatMetric>();
                metric.IntervalStartUtc = intervalStartUtc;
                metric.GameServerId = gameServerId;
                metric.CombatType = key.CombatType;
                metric.MapNumber = key.MapNumber;
                metric.SkillNumber = key.SkillNumber;
                metric.HitCount = snapshot.HitCount;
                metric.HealthDamage = snapshot.HealthDamage;
                metric.ShieldDamage = snapshot.ShieldDamage;
                metric.CriticalHitCount = snapshot.CriticalHitCount;
                metric.ExcellentHitCount = snapshot.ExcellentHitCount;
                metric.DamageBelow200HitCount = snapshot.DamageBelow200HitCount;
                metric.Damage200To399HitCount = snapshot.Damage200To399HitCount;
                metric.Damage400To599HitCount = snapshot.Damage400To599HitCount;
                metric.Damage600To799HitCount = snapshot.Damage600To799HitCount;
                metric.Damage800OrMoreHitCount = snapshot.Damage800OrMoreHitCount;
                metric.AttackerCharacterId = key.CharacterId;
            }

            await context.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            gameContext.LoggerFactory.CreateLogger<CombatMetricsPlugIn>().LogError(ex, "Error while saving combat metrics.");
        }
    }
}
