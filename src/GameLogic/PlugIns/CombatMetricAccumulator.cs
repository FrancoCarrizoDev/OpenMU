// <copyright file="CombatMetricAccumulator.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlugIns;

using System.Threading;

/// <summary>
/// Holds the atomic counters for one combat metric bucket.
/// </summary>
internal sealed class CombatMetricAccumulator
{
    private const uint DamageBucket200 = 200;
    private const uint DamageBucket400 = 400;
    private const uint DamageBucket600 = 600;
    private const uint DamageBucket800 = 800;

    private long _hitCount;
    private long _healthDamage;
    private long _shieldDamage;
    private long _criticalHitCount;
    private long _excellentHitCount;
    private long _damageBelow200HitCount;
    private long _damage200To399HitCount;
    private long _damage400To599HitCount;
    private long _damage600To799HitCount;
    private long _damage800OrMoreHitCount;

    /// <summary>
    /// Adds one successful hit to the aggregate.
    /// </summary>
    public void Add(HitInfo hitInfo)
    {
        Interlocked.Increment(ref this._hitCount);
        Interlocked.Add(ref this._healthDamage, hitInfo.HealthDamage);
        Interlocked.Add(ref this._shieldDamage, hitInfo.ShieldDamage);
        this.AddDamageBucket(hitInfo.HealthDamage + hitInfo.ShieldDamage);

        if (hitInfo.Attributes.HasFlag(DamageAttributes.Critical))
        {
            Interlocked.Increment(ref this._criticalHitCount);
        }

        if (hitInfo.Attributes.HasFlag(DamageAttributes.Excellent))
        {
            Interlocked.Increment(ref this._excellentHitCount);
        }
    }

    /// <summary>
    /// Creates a stable snapshot of the counters.
    /// </summary>
    public CombatMetricSnapshot GetSnapshot()
    {
        return new CombatMetricSnapshot(
            Interlocked.Read(ref this._hitCount),
            Interlocked.Read(ref this._healthDamage),
            Interlocked.Read(ref this._shieldDamage),
            Interlocked.Read(ref this._criticalHitCount),
            Interlocked.Read(ref this._excellentHitCount),
            Interlocked.Read(ref this._damageBelow200HitCount),
            Interlocked.Read(ref this._damage200To399HitCount),
            Interlocked.Read(ref this._damage400To599HitCount),
            Interlocked.Read(ref this._damage600To799HitCount),
            Interlocked.Read(ref this._damage800OrMoreHitCount));
    }

    private void AddDamageBucket(uint damage)
    {
        if (damage < DamageBucket200)
        {
            Interlocked.Increment(ref this._damageBelow200HitCount);
        }
        else if (damage < DamageBucket400)
        {
            Interlocked.Increment(ref this._damage200To399HitCount);
        }
        else if (damage < DamageBucket600)
        {
            Interlocked.Increment(ref this._damage400To599HitCount);
        }
        else if (damage < DamageBucket800)
        {
            Interlocked.Increment(ref this._damage600To799HitCount);
        }
        else
        {
            Interlocked.Increment(ref this._damage800OrMoreHitCount);
        }
    }
}
